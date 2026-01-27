#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Genial.Cms.Application.Commands.Collection;
using CollectionAggregate = Genial.Cms.Domain.Aggregates.Collection;
using Genial.Cms.Domain.Exceptions;
using Genial.Cms.Domain.SeedWork;
using Genial.Cms.Infra.Data.Context;
using MediatR;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Genial.Cms.Application.CommandHandlers.Collection;

public class GetCollectionItemsBySlugCommandHandler : IRequestHandler<GetCollectionItemsBySlugCommand, GetCollectionItemsBySlugResponse>
{
    private readonly ICollectionRepository _collectionRepository;
    private readonly IStageRepository _stageRepository;
    private readonly MongoDbContext _mongoDbContext;
    private readonly IMediator _bus;
    private readonly ILogger<GetCollectionItemsBySlugCommandHandler> _logger;

    public GetCollectionItemsBySlugCommandHandler(
        ICollectionRepository collectionRepository,
        IStageRepository stageRepository,
        MongoDbContext mongoDbContext,
        IMediator bus,
        ILogger<GetCollectionItemsBySlugCommandHandler> logger)
    {
        _collectionRepository = collectionRepository;
        _stageRepository = stageRepository;
        _mongoDbContext = mongoDbContext;
        _bus = bus;
        _logger = logger;
    }

    public async Task<GetCollectionItemsBySlugResponse> Handle(GetCollectionItemsBySlugCommand request, CancellationToken cancellationToken)
    {
        // Validar StageKey
        if (string.IsNullOrWhiteSpace(request.StageKey))
        {
            _logger.LogWarning("StageKey não foi fornecido");
            await _bus.Publish(new ExceptionNotification("064", "StageKey é obrigatório", ExceptionType.Client, "StageKey"), cancellationToken);
            return null!;
        }

        // Validar Slug
        if (string.IsNullOrWhiteSpace(request.Slug))
        {
            _logger.LogWarning("Slug não foi fornecido");
            await _bus.Publish(new ExceptionNotification("064", "Slug é obrigatório", ExceptionType.Client, "Slug"), cancellationToken);
            return null!;
        }

        _logger.LogInformation("Buscando itens da collection pelo stage {StageKey} e slug {Slug}, página {Page}, tamanho {PageSize}", 
            request.StageKey, request.Slug, request.Page, request.PageSize);

        // Buscar o stage pelo key
        var stage = await _stageRepository.GetByKeyAsync(request.StageKey, cancellationToken);
        if (stage == null)
        {
            _logger.LogWarning("Stage não encontrado pelo key: {StageKey}", request.StageKey);
            await _bus.Publish(new ExceptionNotification("065", "Stage não encontrado", ExceptionType.Client, "StageKey"), cancellationToken);
            return null!;
        }

        // Buscar a collection pelo slug e stageId
        var collection = await _collectionRepository.GetBySlugAndStageIdAsync(request.Slug, stage.Id, cancellationToken);
        if (collection == null)
        {
            _logger.LogWarning("Collection não encontrada pelo slug {Slug} e stageId {StageId}", request.Slug, stage.Id);
            await _bus.Publish(new ExceptionNotification("061", "Collection não encontrada", ExceptionType.Client, "Slug"), cancellationToken);
            return null!;
        }

        if (string.IsNullOrWhiteSpace(collection.CollectionName))
        {
            _logger.LogError("Collection não possui CollectionName configurado: {Slug} no stage {StageKey}", request.Slug, request.StageKey);
            await _bus.Publish(new ExceptionNotification("062", "Collection não possui configuração de nome de collection MongoDB", ExceptionType.Server, "CollectionName"), cancellationToken);
            return null!;
        }

        // Buscar itens da collection MongoDB dinâmica
        var mongoCollection = _mongoDbContext.Database.GetCollection<BsonDocument>(collection.CollectionName);

        // Contar total de documentos
        var total = (int)await mongoCollection.CountDocumentsAsync(FilterDefinition<BsonDocument>.Empty, cancellationToken: cancellationToken);

        // Aplicar paginação
        var skip = (request.Page - 1) * request.PageSize;
        var items = await mongoCollection
            .Find(FilterDefinition<BsonDocument>.Empty)
            .SortByDescending(d => d["createdAt"])
            .Skip(skip)
            .Limit(request.PageSize)
            .ToListAsync(cancellationToken);

        // Converter BsonDocument para Dictionary
        var itemsList = items.Select(item => ConvertBsonDocumentToDictionary(item)).ToList();

        var totalPages = (int)Math.Ceiling((double)total / request.PageSize);

        _logger.LogInformation("Itens encontrados: {Count} de {Total} (página {Page} de {TotalPages})", itemsList.Count, total, request.Page, totalPages);

        return new GetCollectionItemsBySlugResponse
        {
            Items = itemsList,
            Total = total,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalPages = totalPages
        };
    }

    private Dictionary<string, object> ConvertBsonDocumentToDictionary(BsonDocument document)
    {
        var result = new Dictionary<string, object>();
        foreach (var element in document.Elements)
        {
            if (element.Name == "_id")
            {
                result["id"] = element.Value.ToString();
            }
            else
            {
                result[element.Name] = ConvertBsonValueToObject(element.Value);
            }
        }
        return result;
    }

    private object ConvertBsonValueToObject(BsonValue bsonValue)
    {
        return bsonValue.BsonType switch
        {
            BsonType.String => bsonValue.AsString,
            BsonType.Int32 => bsonValue.AsInt32,
            BsonType.Int64 => bsonValue.AsInt64,
            BsonType.Double => bsonValue.AsDouble,
            BsonType.Decimal128 => (decimal)bsonValue.AsDecimal128,
            BsonType.Boolean => bsonValue.AsBoolean,
            BsonType.DateTime => bsonValue.ToUniversalTime(),
            BsonType.Null => null!,
            BsonType.Array => bsonValue.AsBsonArray.Select(ConvertBsonValueToObject).ToList(),
            BsonType.Document => bsonValue.AsBsonDocument.ToDictionary(
                e => e.Name,
                e => ConvertBsonValueToObject(e.Value)),
            _ => bsonValue.ToString()
        };
    }
}
