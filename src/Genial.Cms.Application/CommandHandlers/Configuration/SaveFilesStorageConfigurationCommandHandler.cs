using System;
using System.Threading;
using System.Threading.Tasks;
using Genial.Cms.Application.Commands.Configuration;
using Genial.Cms.Domain.Exceptions;
using Genial.Cms.Domain.SeedWork;
using MediatR;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using ConfigurationAggregate = Genial.Cms.Domain.Aggregates.Configuration;

namespace Genial.Cms.Application.CommandHandlers.Configuration;

public class SaveFilesStorageConfigurationCommandHandler : IRequestHandler<SaveFilesStorageConfigurationCommand, SaveFilesStorageConfigurationCommandResult>
{
    private readonly IConfigurationRepository _configurationRepository;
    private readonly IMediator _bus;
    private readonly ILogger<SaveFilesStorageConfigurationCommandHandler> _logger;

    public SaveFilesStorageConfigurationCommandHandler(
        IConfigurationRepository configurationRepository,
        IMediator bus,
        ILogger<SaveFilesStorageConfigurationCommandHandler> logger)
    {
        _configurationRepository = configurationRepository;
        _bus = bus;
        _logger = logger;
    }

    public async Task<SaveFilesStorageConfigurationCommandResult> Handle(SaveFilesStorageConfigurationCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Salvando configuração FilesStorage");

        const string configKey = "filesStorage";
        var existingConfiguration = await _configurationRepository.GetByKeyAsync(configKey, cancellationToken);

        var now = DateTime.UtcNow;
        ConfigurationAggregate configuration;

        if (existingConfiguration != null)
        {
            // Atualizar configuração existente
            _logger.LogInformation("Atualizando configuração FilesStorage existente");
            
            existingConfiguration.Status = request.Status;
            existingConfiguration.Values = new BsonDocument
            {
                { "bucket", request.Values.Bucket ?? "" },
                { "region", request.Values.Region ?? "" },
                { "endpoint", string.IsNullOrWhiteSpace(request.Values.Endpoint) ? BsonNull.Value : request.Values.Endpoint },
                { "accessKeyId", request.Values.AccessKeyId ?? "" },
                { "secretAccessKey", request.Values.SecretAccessKey ?? "" },
                { "folder", request.Values.Folder ?? "" }
            };
            existingConfiguration.UpdatedAt = now;

            await _configurationRepository.UpdateAsync(existingConfiguration, cancellationToken);
            configuration = existingConfiguration;
        }
        else
        {
            // Criar nova configuração
            _logger.LogInformation("Criando nova configuração FilesStorage");
            
            configuration = new ConfigurationAggregate
            {
                Key = configKey,
                Status = request.Status,
                Values = new BsonDocument
                {
                    { "bucket", request.Values.Bucket ?? "" },
                    { "region", request.Values.Region ?? "" },
                    { "endpoint", string.IsNullOrWhiteSpace(request.Values.Endpoint) ? BsonNull.Value : request.Values.Endpoint },
                    { "accessKeyId", request.Values.AccessKeyId ?? "" },
                    { "secretAccessKey", request.Values.SecretAccessKey ?? "" },
                    { "folder", request.Values.Folder ?? "" }
                },
                CreatedAt = now,
                UpdatedAt = now
            };

            await _configurationRepository.InsertAsync(configuration, cancellationToken);
        }

        _logger.LogInformation("Configuração FilesStorage salva com sucesso. Id: {Id}", configuration.Id);

        // Converter BsonDocument para DTO
        var valuesDto = new FilesStorageValuesDto();
        if (configuration.Values != null)
        {
            valuesDto.Bucket = configuration.Values.GetValue("bucket", "").AsString;
            valuesDto.Region = configuration.Values.GetValue("region", "").AsString;
            var endpointValue = configuration.Values.GetValue("endpoint", BsonNull.Value);
            valuesDto.Endpoint = endpointValue.IsBsonNull ? null : endpointValue.AsString;
            valuesDto.AccessKeyId = configuration.Values.GetValue("accessKeyId", "").AsString;
            valuesDto.SecretAccessKey = configuration.Values.GetValue("secretAccessKey", "").AsString;
            valuesDto.Folder = configuration.Values.GetValue("folder", "").AsString;
        }

        return new SaveFilesStorageConfigurationCommandResult
        {
            Id = configuration.Id,
            Key = configuration.Key,
            Status = configuration.Status,
            Values = valuesDto,
            CreatedAt = configuration.CreatedAt,
            UpdatedAt = configuration.UpdatedAt
        };
    }
}
