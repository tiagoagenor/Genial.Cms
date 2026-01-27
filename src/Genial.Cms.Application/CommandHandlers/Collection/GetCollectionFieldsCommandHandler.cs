using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Genial.Cms.Application.Commands.Collection;
using Genial.Cms.Domain.Exceptions;
using Genial.Cms.Domain.SeedWork;
using MediatR;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;

namespace Genial.Cms.Application.CommandHandlers.Collection;

public class GetCollectionFieldsCommandHandler : IRequestHandler<GetCollectionFieldsCommand, GetCollectionFieldsCommandResult>
{
    private readonly ICollectionRepository _collectionRepository;
    private readonly IMediator _bus;
    private readonly ILogger<GetCollectionFieldsCommandHandler> _logger;

    public GetCollectionFieldsCommandHandler(
        ICollectionRepository collectionRepository,
        IMediator bus,
        ILogger<GetCollectionFieldsCommandHandler> logger)
    {
        _collectionRepository = collectionRepository;
        _bus = bus;
        _logger = logger;
    }

    public async Task<GetCollectionFieldsCommandResult> Handle(GetCollectionFieldsCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Buscando fields da collection: {CollectionId}", request.CollectionId);

        // Buscar a collection pelo ID
        var collection = await _collectionRepository.GetByIdAsync(request.CollectionId, cancellationToken);

        if (collection == null)
        {
            _logger.LogWarning("Collection não encontrada. Id: {CollectionId}", request.CollectionId);
            await _bus.Publish(new ExceptionNotification("066", "Collection não encontrada.", ExceptionType.Client, "CollectionId"), cancellationToken);
            return null;
        }

        _logger.LogInformation("Collection encontrada. Id: {CollectionId}, Name: {Name}, Fields: {Count}", 
            collection.Id, collection.Name, collection.Fields?.Count ?? 0);

        // Converter os fields para DTOs
        var fields = new List<CollectionFieldDto>();

        if (collection.Fields != null && collection.Fields.Count > 0)
        {
            foreach (var field in collection.Fields)
            {
                // Converter BsonDocument para objeto
                object data = null;
                if (field.Data != null)
                {
                    try
                    {
                        var jsonString = field.Data.ToJson();
                        data = JsonSerializer.Deserialize<object>(jsonString, new JsonSerializerOptions
                        {
                            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                        });
                    }
                    catch (System.Exception ex)
                    {
                        _logger.LogWarning(ex, "Erro ao deserializar data do field. Type: {Type}, Name: {Name}", field.Type, field.Name);
                        data = new { };
                    }
                }

                fields.Add(new CollectionFieldDto
                {
                    Type = field.Type,
                    Name = field.Name,
                    Slug = field.Slug,
                    Data = data,
                    CreatedAt = field.CreatedAt,
                    UpdatedAt = field.UpdatedAt
                });
            }
        }

        var result = new GetCollectionFieldsCommandResult
        {
            CollectionId = collection.Id,
            CollectionName = collection.Name,
            Fields = fields
        };

        _logger.LogInformation("Retornando {Count} fields da collection {CollectionId}", fields.Count, request.CollectionId);

        return result;
    }
}
