using System;
using System.Threading;
using System.Threading.Tasks;
using Genial.Cms.Application.Commands.Configuration;
using Genial.Cms.Domain.Exceptions;
using Genial.Cms.Domain.SeedWork;
using MediatR;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;

namespace Genial.Cms.Application.CommandHandlers.Configuration;

public class GetFilesStorageConfigurationCommandHandler : IRequestHandler<GetFilesStorageConfigurationCommand, GetFilesStorageConfigurationCommandResult>
{
    private readonly IConfigurationRepository _configurationRepository;
    private readonly IMediator _bus;
    private readonly ILogger<GetFilesStorageConfigurationCommandHandler> _logger;

    public GetFilesStorageConfigurationCommandHandler(
        IConfigurationRepository configurationRepository,
        IMediator bus,
        ILogger<GetFilesStorageConfigurationCommandHandler> logger)
    {
        _configurationRepository = configurationRepository;
        _bus = bus;
        _logger = logger;
    }

    public async Task<GetFilesStorageConfigurationCommandResult> Handle(GetFilesStorageConfigurationCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Buscando configuração FilesStorage");

        const string configKey = "filesStorage";
        var configuration = await _configurationRepository.GetByKeyAsync(configKey, cancellationToken);

        if (configuration == null)
        {
            _logger.LogInformation("Configuração FilesStorage não encontrada, retornando valores padrão");
            
            // Retornar valores padrão
            return new GetFilesStorageConfigurationCommandResult
            {
                Key = configKey,
                Status = false,
                Values = new GetFilesStorageValuesDto
                {
                    Bucket = "",
                    Region = "",
                    Endpoint = null,
                    AccessKeyId = "",
                    SecretAccessKey = "",
                    Folder = "uploads"
                }
            };
        }

        // Converter BsonDocument para DTO
        var valuesDto = new GetFilesStorageValuesDto();
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

        return new GetFilesStorageConfigurationCommandResult
        {
            Key = configuration.Key,
            Status = configuration.Status,
            Values = valuesDto
        };
    }
}
