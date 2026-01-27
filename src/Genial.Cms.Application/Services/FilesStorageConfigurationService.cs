using System;
using System.Threading;
using System.Threading.Tasks;
using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Genial.Cms.Domain.SeedWork;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;

namespace Genial.Cms.Application.Services;

public class FilesStorageConfigurationService : IFilesStorageConfigurationService
{
    private const string ConfigKey = "filesStorage";
    private const string CacheKey = "FilesStorageConfiguration";
    private readonly IConfigurationRepository _configurationRepository;
    private readonly IMemoryCache _cache;
    private readonly ILogger<FilesStorageConfigurationService> _logger;
    private static readonly TimeSpan CacheExpiration = TimeSpan.FromMinutes(5);

    public FilesStorageConfigurationService(
        IConfigurationRepository configurationRepository,
        IMemoryCache cache,
        ILogger<FilesStorageConfigurationService> logger)
    {
        _configurationRepository = configurationRepository;
        _cache = cache;
        _logger = logger;
    }

    public async Task<FilesStorageConfiguration> GetConfigurationAsync(CancellationToken cancellationToken = default)
    {
        // Tentar buscar do cache primeiro
        if (_cache.TryGetValue(CacheKey, out FilesStorageConfiguration cachedConfig))
        {
            _logger.LogDebug("Configuração FilesStorage retornada do cache");
            return cachedConfig;
        }

        // Buscar do banco de dados
        var configuration = await _configurationRepository.GetByKeyAsync(ConfigKey, cancellationToken);

        FilesStorageConfiguration result;

        if (configuration == null || !configuration.Status)
        {
            _logger.LogInformation("Configuração FilesStorage não encontrada ou desabilitada, retornando valores padrão");
            
            // Retornar valores padrão
            result = new FilesStorageConfiguration
            {
                Status = false,
                Bucket = string.Empty,
                Region = string.Empty,
                Endpoint = null,
                AccessKeyId = string.Empty,
                SecretAccessKey = string.Empty,
                Folder = "uploads"
            };
        }
        else
        {
            // Converter BsonDocument para objeto
            result = new FilesStorageConfiguration
            {
                Status = configuration.Status,
                Bucket = configuration.Values?.GetValue("bucket", "").AsString ?? string.Empty,
                Region = configuration.Values?.GetValue("region", "").AsString ?? string.Empty,
                Endpoint = GetEndpointValue(configuration.Values),
                AccessKeyId = configuration.Values?.GetValue("accessKeyId", "").AsString ?? string.Empty,
                SecretAccessKey = configuration.Values?.GetValue("secretAccessKey", "").AsString ?? string.Empty,
                Folder = configuration.Values?.GetValue("folder", "").AsString ?? "uploads"
            };
        }

        // Armazenar no cache
        _cache.Set(CacheKey, result, CacheExpiration);
        _logger.LogInformation("Configuração FilesStorage carregada do banco e armazenada no cache");

        return result;
    }

    public bool IsEnabled()
    {
        // Verificar cache primeiro
        if (_cache.TryGetValue(CacheKey, out FilesStorageConfiguration cachedConfig))
        {
            return cachedConfig.Status;
        }

        // Se não estiver no cache, retornar false (será carregado quando necessário)
        return false;
    }

    public async Task<bool> TestConnectionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // Invalidar cache para garantir que busca as configurações mais recentes do banco
            _cache.Remove(CacheKey);
            
            // Buscar configuração diretamente do banco (sem cache)
            var configuration = await _configurationRepository.GetByKeyAsync(ConfigKey, cancellationToken);

            // Verificar se existe e está habilitado
            if (configuration == null || !configuration.Status)
            {
                _logger.LogWarning("S3 está desabilitado ou não configurado");
                return false;
            }

            // Extrair valores do BsonDocument
            var bucket = configuration.Values?.GetValue("bucket", "").AsString ?? string.Empty;
            var region = configuration.Values?.GetValue("region", "").AsString ?? string.Empty;
            var accessKeyId = configuration.Values?.GetValue("accessKeyId", "").AsString ?? string.Empty;
            var secretAccessKey = configuration.Values?.GetValue("secretAccessKey", "").AsString ?? string.Empty;
            var endpoint = GetEndpointValue(configuration.Values);

            // Verificar se as credenciais estão configuradas
            if (string.IsNullOrWhiteSpace(bucket) || 
                string.IsNullOrWhiteSpace(region) ||
                string.IsNullOrWhiteSpace(accessKeyId) ||
                string.IsNullOrWhiteSpace(secretAccessKey))
            {
                _logger.LogWarning("Configuração do S3 incompleta. Bucket, Region, AccessKeyId e SecretAccessKey são obrigatórios");
                return false;
            }

            // Tentar conectar ao S3 usando AWS SDK
            try
            {
                var s3Config = new Amazon.S3.AmazonS3Config
                {
                    RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(region)
                };

                // Se tiver endpoint customizado (ex: MinIO, LocalStack), usar
                if (!string.IsNullOrWhiteSpace(endpoint))
                {
                    s3Config.ServiceURL = endpoint;
                    s3Config.ForcePathStyle = true; // Necessário para MinIO e compatíveis
                }

                using var s3Client = new Amazon.S3.AmazonS3Client(
                    accessKeyId,
                    secretAccessKey,
                    s3Config
                );

                // Tentar fazer uma operação que realmente valida as credenciais
                // ListObjectsV2 valida credenciais e acesso ao bucket
                var listRequest = new Amazon.S3.Model.ListObjectsV2Request
                {
                    BucketName = bucket,
                    MaxKeys = 1 // Apenas verificar se consegue acessar, não precisa listar tudo
                };

                await s3Client.ListObjectsV2Async(listRequest, cancellationToken);
                
                _logger.LogInformation("Conexão com S3 testada com sucesso. Bucket: {Bucket}, Region: {Region}", bucket, region);
                return true;
            }
            catch (Amazon.S3.AmazonS3Exception s3Ex)
            {
                _logger.LogError(s3Ex, "Erro ao conectar com S3. StatusCode: {StatusCode}, ErrorCode: {ErrorCode}, Message: {Message}", 
                    s3Ex.StatusCode, s3Ex.ErrorCode, s3Ex.Message);
                return false;
            }
            catch (Amazon.Runtime.AmazonServiceException awsEx)
            {
                _logger.LogError(awsEx, "Erro de serviço AWS ao testar S3. StatusCode: {StatusCode}, ErrorCode: {ErrorCode}, Message: {Message}", 
                    awsEx.StatusCode, awsEx.ErrorCode, awsEx.Message);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado ao testar conexão com S3: {Message}, StackTrace: {StackTrace}", 
                    ex.Message, ex.StackTrace);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao testar conexão com S3: {Message}", ex.Message);
            return false;
        }
    }

    private string? GetEndpointValue(BsonDocument? values)
    {
        if (values == null)
            return null;

        var endpointValue = values.GetValue("endpoint", BsonNull.Value);
        return endpointValue.IsBsonNull ? null : endpointValue.AsString;
    }
}
