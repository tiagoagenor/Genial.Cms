using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Genial.Cms.Application.Commands.Collection;
using Genial.Cms.Domain.Aggregates;
using Genial.Cms.Domain.Exceptions;
using Genial.Cms.Domain.SeedWork;
using System.Linq;
using System.Text.RegularExpressions;
using MediatR;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using CollectionAggregate = Genial.Cms.Domain.Aggregates.Collection;

namespace Genial.Cms.Application.CommandHandlers.Collection;

public class CreateCollectionCommandHandler : IRequestHandler<CreateCollectionCommand, CreateCollectionCommandResult>
{
    private readonly ICollectionRepository _collectionRepository;
    private readonly IMediator _bus;
    private readonly ILogger<CreateCollectionCommandHandler> _logger;

    public CreateCollectionCommandHandler(
        ICollectionRepository collectionRepository,
        IMediator bus,
        ILogger<CreateCollectionCommandHandler> logger)
    {
        _collectionRepository = collectionRepository;
        _bus = bus;
        _logger = logger;
    }

    public async Task<CreateCollectionCommandResult> Handle(CreateCollectionCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Iniciando criação de collection '{Name}' com {Count} campos", request.Name, request.Fields?.Count ?? 0);

        // Validar dados do usuário logado
        if (request.UserData == null || string.IsNullOrWhiteSpace(request.UserData.StageId))
        {
            _logger.LogWarning("StageId não encontrado no token do usuário");
            await _bus.Publish(new ExceptionNotification("065", "Não foi possível identificar o stage do usuário. Token inválido.", ExceptionType.Client, "StageId"), cancellationToken);
            return null;
        }

        // Validar Name
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            _logger.LogWarning("Name não foi informado na requisição");
            await _bus.Publish(new ExceptionNotification("063", "O campo 'name' é obrigatório.", ExceptionType.Client, "Name"), cancellationToken);
            return null;
        }

        // Validar se o name já existe no mesmo stage
        // Se for do mesmo stage e o nome for igual, não deixar criar
        // Se for de stage diferente, pode criar mesmo com nome igual
        var existingCollectionByNameAndStage = await _collectionRepository.GetByNameAndStageIdAsync(request.Name, request.UserData.StageId, cancellationToken);
        if (existingCollectionByNameAndStage != null)
        {
            _logger.LogWarning("Nome já existe no mesmo stage. Name: {Name}, StageId: {StageId}", request.Name, request.UserData.StageId);
            await _bus.Publish(new ExceptionNotification("064", $"Já existe uma collection com o nome '{request.Name}' no mesmo stage.", ExceptionType.Client, "Name"), cancellationToken);
            return null;
        }

        if (request.Fields == null || request.Fields.Count == 0)
        {
            _logger.LogWarning("Nenhum field foi enviado na requisição");
            await _bus.Publish(new ExceptionNotification("058", "É necessário enviar pelo menos um campo.", ExceptionType.Client, "Fields"), cancellationToken);
            return null;
        }

        // Processar cada campo
        var collectionFields = new List<CollectionField>();
        var fieldResults = new List<CollectionFieldItemResultDto>();
        var nomesProcessados = new HashSet<string>();
        var now = DateTime.UtcNow;

        foreach (var field in request.Fields)
        {
            _logger.LogInformation("Processando field. Type: {Type}, Name: {Name}", field.Type, field.Name);

            // Validar se o nome já existe na requisição atual
            if (nomesProcessados.Contains(field.Name))
            {
                _logger.LogWarning("Nome duplicado encontrado na requisição: {Name}", field.Name);
                await _bus.Publish(new ExceptionNotification("061", $"Já existe um campo com o nome '{field.Name}'. Nomes devem ser únicos.", ExceptionType.Client, "Name"), cancellationToken);
                return null;
            }

            nomesProcessados.Add(field.Name);

            // Selecionar apenas a Data correspondente ao Type
            object data = field.Type.ToLower() switch
            {
                "file" => field.FileData ?? new FileDataDto(),
                "input" => field.InputData ?? new InputDataDto(),
                "text" => field.TextData ?? new TextDataDto(),
                "number" => field.NumberData ?? new NumberDataDto(),
                "email" => field.EmailData ?? new EmailDataDto(),
                "select" => field.SelectData ?? new SelectDataDto(),
                "radio" => field.RadioData ?? new RadioDataDto(),
                "bool" => field.BoolData ?? new BoolDataDto { Required = false },
                "checkbox" => field.CheckboxData ?? new CheckboxDataDto(),
                "range" => field.RangeData ?? new RangeDataDto(),
                "color" => field.ColorData ?? new ColorDataDto { Required = false },
                _ => new object()
            };

            _logger.LogInformation("Data selecionada para Type {Type}: {Data}", field.Type, JsonSerializer.Serialize(data));

            // Serializar apenas a Data correta para BsonDocument
            BsonDocument bsonDocument;
            try
            {
                var jsonString = JsonSerializer.Serialize(data, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.Never
                });
                _logger.LogInformation("JSON serializado: {Json}", jsonString);
                bsonDocument = BsonDocument.Parse(jsonString);
                _logger.LogInformation("BsonDocument criado com sucesso");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao serializar data para BsonDocument. Type: {Type}", field.Type);
                await _bus.Publish(new ExceptionNotification("059", $"Erro ao processar dados do campo: {ex.Message}", ExceptionType.Server, "Data"), cancellationToken);
                return null;
            }

            // Gerar slug único para o field
            var fieldSlug = await GenerateUniqueFieldSlugAsync(field.Name, cancellationToken);

            // Criar CollectionField (sem Id, será ignorado quando dentro da Collection)
            var collectionField = new CollectionField
            {
                Id = null, // Será ignorado pelo BsonIgnoreIfDefault
                Type = field.Type,
                Name = field.Name,
                Slug = fieldSlug,
                Data = bsonDocument,
                CreatedAt = now,
                UpdatedAt = now
            };

            collectionFields.Add(collectionField);

            // Adicionar ao resultado
            fieldResults.Add(new CollectionFieldItemResultDto
            {
                Id = null, // Fields dentro da collection não têm Id
                Type = field.Type,
                Name = field.Name,
                Slug = fieldSlug,
                Data = data
            });
        }

        // Gerar slug único para a collection
        var collectionSlug = await GenerateUniqueCollectionSlugAsync(request.Name, request.UserData.StageId, cancellationToken);

        // Criar a Collection com os fields dentro
        var collection = new CollectionAggregate
        {
            Name = request.Name,
            Slug = collectionSlug,
            StageId = request.UserData.StageId,
            Fields = collectionFields,
            CreatedAt = now,
            UpdatedAt = now
        };

        _logger.LogInformation("Preparando para inserir collection '{Name}' com {Count} campos no MongoDB", collection.Name, collectionFields.Count);

        try
        {
            // Verificar se o repository está configurado
            if (_collectionRepository == null)
            {
                _logger.LogError("CollectionRepository é null!");
                await _bus.Publish(new ExceptionNotification("060", "Erro de configuração: Repository não inicializado.", ExceptionType.Server, "Repository"), cancellationToken);
                return null;
            }

            // Inserir a collection única com os fields dentro
            await _collectionRepository.InsertAsync(collection, cancellationToken);
            _logger.LogInformation("Collection inserida com sucesso. Id: {CollectionId}, Name: {Name}, Slug: {Slug}",
                collection.Id, collection.Name, collection.Slug);

            // Gerar nome único para a collection MongoDB: {stageKey}_{slug_collection}
            var mongoCollectionName = await GenerateUniqueMongoCollectionNameAsync(
                request.UserData.StageKey,
                collectionSlug,
                request.UserData.StageId,
                cancellationToken);

            // Criar a collection no MongoDB
            var created = await _collectionRepository.CreateMongoCollectionAsync(mongoCollectionName, cancellationToken);
            if (created)
            {
                _logger.LogInformation("Collection MongoDB criada com sucesso. Nome: {CollectionName}", mongoCollectionName);
            }
            else
            {
                _logger.LogWarning("Collection MongoDB '{CollectionName}' já existe ou houve erro ao criar", mongoCollectionName);
            }

            // Atualizar a collection principal com o nome da collection MongoDB criada
            collection.CollectionName = mongoCollectionName;
            await _collectionRepository.UpdateAsync(collection, cancellationToken);
            _logger.LogInformation("Collection atualizada com nome da collection MongoDB. CollectionName: {CollectionName}", mongoCollectionName);

            _logger.LogInformation("Collection criada com sucesso. {Count} campos salvos", collectionFields.Count);

            return new CreateCollectionCommandResult
            {
                Id = collection.Id,
                Fields = fieldResults,
                CreatedAt = collection.CreatedAt,
                UpdatedAt = collection.UpdatedAt
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao inserir collection. Exception: {Exception}", ex.ToString());
            await _bus.Publish(new ExceptionNotification("057", $"Erro ao salvar collection: {ex.Message}", ExceptionType.Server, "Collection"), cancellationToken);
            return null;
        }
    }

    private async Task<string> GenerateUniqueCollectionSlugAsync(string name, string stageId, CancellationToken cancellationToken)
    {
        // Gerar slug base a partir do name
        var baseSlug = CollectionAggregate.GenerateSlug(name);
        var slug = baseSlug;
        var counter = 1;

        // Verificar se o slug base já existe no mesmo stage
        var existingCollection = await _collectionRepository.GetBySlugAndStageIdAsync(slug, stageId, cancellationToken);

        if (existingCollection != null)
        {
            // Verificar se o slug existente termina com _numero
            var existingSlug = existingCollection.Slug;
            var match = Regex.Match(existingSlug, @"^(.+)_(\d+)$");

            if (match.Success)
            {
                // Se termina com _numero, extrair o número e incrementar
                var basePart = match.Groups[1].Value;
                var numberPart = int.Parse(match.Groups[2].Value);
                counter = numberPart + 1;
                slug = $"{basePart}_{counter}";
            }
            else
            {
                // Se não termina com _numero, adicionar _1
                slug = $"{baseSlug}_1";
                counter = 1;
            }

            // Continuar incrementando até encontrar um slug disponível no mesmo stage
            while (true)
            {
                existingCollection = await _collectionRepository.GetBySlugAndStageIdAsync(slug, stageId, cancellationToken);
                if (existingCollection == null)
                {
                    break; // Slug disponível encontrado
                }

                // Verificar se o slug existente encontrado também termina com _numero
                var existingSlugCheck = existingCollection.Slug;
                var matchCheck = Regex.Match(existingSlugCheck, @"^(.+)_(\d+)$");

                if (matchCheck.Success)
                {
                    // Se termina com _numero, usar esse número como base para incrementar
                    var numberPartCheck = int.Parse(matchCheck.Groups[2].Value);
                    counter = numberPartCheck + 1;
                }
                else
                {
                    // Se não termina com _numero, apenas incrementar o contador
                    counter++;
                }

                slug = $"{baseSlug}_{counter}";
            }
        }

        // Garantir que o slug final seja sempre minúsculo
        slug = slug?.ToLowerInvariant() ?? string.Empty;

        _logger.LogInformation("Slug único da collection gerado: {Slug} (base: {BaseSlug}, stageId: {StageId})", slug, baseSlug, stageId);
        return slug;
    }

    private async Task<string> GenerateUniqueFieldSlugAsync(string name, CancellationToken cancellationToken)
    {
        // Para fields dentro de uma collection, gerar slug simples baseado no nome
        // Como os fields não são salvos separadamente, não precisamos verificar duplicatas
        // Mas ainda geramos um slug único dentro do contexto da collection atual
        var baseSlug = CollectionField.GenerateSlug(name);
        // Garantir que seja minúsculo (redundante, mas garante consistência)
        var slug = baseSlug?.ToLowerInvariant() ?? string.Empty;

        // Como os fields são salvos dentro da collection, não precisamos verificar no banco
        // Mas podemos adicionar um sufixo numérico se houver duplicatas na mesma requisição
        // Isso será tratado pela validação de nomes duplicados na requisição

        _logger.LogInformation("Slug do field gerado: {Slug}", slug);
        return slug;
    }

    private async Task<string> GenerateUniqueMongoCollectionNameAsync(string stageKey, string collectionSlug, string stageId, CancellationToken cancellationToken)
    {
        // Garantir que stageKey e collectionSlug sejam minúsculos
        var stageKeyLower = stageKey?.ToLowerInvariant() ?? string.Empty;
        var collectionSlugLower = collectionSlug?.ToLowerInvariant() ?? string.Empty;

        // Gerar nome base: {stageKey}_{slug_collection}
        // Como o nome já inclui o stageKey, collections de stages diferentes não conflitam
        var baseName = $"{stageKeyLower}_{collectionSlugLower}";
        var collectionName = baseName;

        // Verificar se o nome já existe APENAS no mesmo stage
        // Collections de stages diferentes podem ter o mesmo nome de collection MongoDB
        // porque o nome já inclui o stageKey (dev_teste vs hml_teste são diferentes)
        var existingCollection = await _collectionRepository.GetByCollectionNameAndStageIdAsync(collectionName, stageId, cancellationToken);

        if (existingCollection != null)
        {
            // Se existe no mesmo stage, adicionar letra no final (a, b, c, etc.)
            var suffix = 'a';
            while (suffix <= 'z')
            {
                collectionName = $"{baseName}{suffix}";
                existingCollection = await _collectionRepository.GetByCollectionNameAndStageIdAsync(collectionName, stageId, cancellationToken);

                if (existingCollection == null)
                {
                    break; // Nome disponível encontrado
                }

                // Incrementar para próxima letra
                suffix++;
            }

            // Se passou de 'z' e ainda não encontrou, usar números
            if (suffix > 'z')
            {
                var counter = 1;
                while (true)
                {
                    collectionName = $"{baseName}{counter}";
                    existingCollection = await _collectionRepository.GetByCollectionNameAndStageIdAsync(collectionName, stageId, cancellationToken);

                    if (existingCollection == null)
                    {
                        break; // Nome disponível encontrado
                    }

                    counter++;
                }
            }
        }

        // Garantir que o nome da collection MongoDB seja sempre minúsculo
        collectionName = collectionName?.ToLowerInvariant() ?? string.Empty;

        _logger.LogInformation("Nome único da collection MongoDB gerado: {CollectionName} (base: {BaseName}, stageId: {StageId})", collectionName, baseName, stageId);
        return collectionName;
    }

}
