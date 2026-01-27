#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Genial.Cms.Application.Commands.Collection;
using CollectionAggregate = Genial.Cms.Domain.Aggregates.Collection;
using Genial.Cms.Domain.Aggregates;
using Genial.Cms.Domain.Exceptions;
using Genial.Cms.Domain.SeedWork;
using Genial.Cms.Infra.Data.Context;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Genial.Cms.Application.CommandHandlers.Collection;

public class GetCollectionItemsCommandHandler : IRequestHandler<GetCollectionItemsCommand, GetCollectionItemsResponse>
{
    private readonly ICollectionRepository _collectionRepository;
    private readonly MongoDbContext _mongoDbContext;
    private readonly IMediaRepository _mediaRepository;
    private readonly IConfiguration _configuration;
    private readonly IMediator _bus;
    private readonly ILogger<GetCollectionItemsCommandHandler> _logger;

    public GetCollectionItemsCommandHandler(
        ICollectionRepository collectionRepository,
        MongoDbContext mongoDbContext,
        IMediaRepository mediaRepository,
        IConfiguration configuration,
        IMediator bus,
        ILogger<GetCollectionItemsCommandHandler> logger)
    {
        _collectionRepository = collectionRepository;
        _mongoDbContext = mongoDbContext;
        _mediaRepository = mediaRepository;
        _configuration = configuration;
        _bus = bus;
        _logger = logger;
    }

    public async Task<GetCollectionItemsResponse> Handle(GetCollectionItemsCommand request, CancellationToken cancellationToken)
    {
        // Validar CollectionId
        if (string.IsNullOrWhiteSpace(request.CollectionId))
        {
            _logger.LogWarning("CollectionId não foi fornecido");
            await _bus.Publish(new ExceptionNotification("064", "CollectionId é obrigatório", ExceptionType.Client, "CollectionId"), cancellationToken);
            return null!;
        }

        _logger.LogInformation("Buscando itens da collection {CollectionId}, página {Page}, tamanho {PageSize}", request.CollectionId, request.Page, request.PageSize);

        // Buscar a collection
        var collection = await _collectionRepository.GetByIdAsync(request.CollectionId, cancellationToken);
        if (collection == null)
        {
            _logger.LogWarning("Collection não encontrada: {CollectionId}", request.CollectionId);
            await _bus.Publish(new ExceptionNotification("061", "Collection não encontrada", ExceptionType.Client, "CollectionId"), cancellationToken);
            return null!;
        }

        if (string.IsNullOrWhiteSpace(collection.CollectionName))
        {
            _logger.LogError("Collection não possui CollectionName configurado: {CollectionId}", request.CollectionId);
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

        // Fazer join com _media para campos do tipo "file"
        if (collection.Fields != null && collection.Fields.Count > 0)
        {
            var fileFields = collection.Fields.Where(f => 
                !string.IsNullOrWhiteSpace(f.Type) && 
                f.Type.Equals("file", System.StringComparison.OrdinalIgnoreCase) &&
                !string.IsNullOrWhiteSpace(f.Slug)).ToList();

            // Processar cada item da lista
            foreach (var itemDictionary in itemsList)
            {
                foreach (var fileField in fileFields)
                {
                    // Verificar se o campo existe no item e tem um valor
                    if (itemDictionary.ContainsKey(fileField.Slug) && itemDictionary[fileField.Slug] != null)
                    {
                        var fieldValue = itemDictionary[fileField.Slug];
                        string? mediaId = null;

                        // Extrair o valor do campo (pode ser ID, fileNameUrl, URL completa ou objeto)
                        string? fieldValueString = null;
                        if (fieldValue is string stringValue)
                        {
                            fieldValueString = stringValue;
                        }
                        else if (fieldValue is Dictionary<string, object> dictValue)
                        {
                            if (dictValue.ContainsKey("id"))
                                fieldValueString = dictValue["id"]?.ToString();
                            else if (dictValue.ContainsKey("_id"))
                                fieldValueString = dictValue["_id"]?.ToString();
                            else if (dictValue.ContainsKey("fileNameUrl"))
                                fieldValueString = dictValue["fileNameUrl"]?.ToString();
                            else if (dictValue.ContainsKey("url"))
                                fieldValueString = dictValue["url"]?.ToString();
                        }

                        // Buscar o media no MongoDB usando múltiplas estratégias
                        if (!string.IsNullOrWhiteSpace(fieldValueString))
                        {
                            try
                            {
                                Media? media = null;

                                // Estratégia 1: Tentar buscar pelo ID (ObjectId do MongoDB)
                                if (MongoDB.Bson.ObjectId.TryParse(fieldValueString, out _))
                                {
                                    media = await _mediaRepository.GetByIdAsync(fieldValueString, cancellationToken);
                                }

                                // Estratégia 2: Se não encontrou, tentar buscar pela URL completa
                                if (media is null && fieldValueString.StartsWith("http", System.StringComparison.OrdinalIgnoreCase))
                                {
                                    media = await _mediaRepository.GetByUrlAsync(fieldValueString, cancellationToken);
                                }

                                // Estratégia 3: Se não encontrou, tentar buscar pelo fileNameUrl (UUID.extensão)
                                if (media is null)
                                {
                                    media = await _mediaRepository.GetByFileNameUrlAsync(fieldValueString, cancellationToken);
                                }

                                // Estratégia 4: Se ainda não encontrou, tentar construir a URL e buscar
                                if (media is null)
                                {
                                    var baseUrl = _configuration["FileUpload:BaseUrl"] ?? "http://localhost:5000/v1/files/upload";
                                    var fullUrl = $"{baseUrl.TrimEnd('/')}/{fieldValueString}";
                                    media = await _mediaRepository.GetByUrlAsync(fullUrl, cancellationToken);
                                }

                                // Se encontrou o media, substituir o valor pelo objeto completo
                                if (media is not null)
                                {
                                    // Formato simplificado (sem $, valores diretos)
                                    itemDictionary[fileField.Slug] = ConvertMediaToMongoDictionary(media);
                                }
                            }
                            catch (System.Exception ex)
                            {
                                _logger.LogWarning(ex, "Erro ao buscar media para o valor: {Value} no campo {FieldSlug}", fieldValueString, fileField.Slug);
                                // Continuar mesmo se houver erro ao buscar o media
                            }
                        }
                    }
                }
            }
        }

        // Converter fields para columns
        var columns = new List<CollectionColumnDto>();
        if (collection.Fields != null && collection.Fields.Count > 0)
        {
            columns = collection.Fields.Select(field => new CollectionColumnDto
            {
                Id = field.Id ?? string.Empty,
                Type = field.Type ?? string.Empty,
                Nome = field.Name ?? string.Empty,
                Slug = field.Slug ?? string.Empty
            }).ToList();
        }

        var totalPages = (int)Math.Ceiling((double)total / request.PageSize);

        _logger.LogInformation("Itens encontrados: {Count} de {Total} (página {Page} de {TotalPages})", itemsList.Count, total, request.Page, totalPages);

        return new GetCollectionItemsResponse
        {
            CollectionName = collection.Name,
            CollectionSlug = collection.Slug ?? string.Empty,
            Columns = columns,
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

    /// <summary>
    /// Converte um objeto Media para Dictionary no formato simplificado (sem $, valores diretos)
    /// </summary>
    private Dictionary<string, object> ConvertMediaToMongoDictionary(Media media)
    {
        return new Dictionary<string, object>
        {
            ["_id"] = media.Id,
            ["fileName"] = media.FileName,
            ["fileNameUrl"] = media.FileNameUrl ?? string.Empty,
            ["contentType"] = media.ContentType,
            ["fileSize"] = media.FileSize,
            ["url"] = media.Url,
            ["tags"] = media.Tags ?? new List<string>(),
            ["extension"] = media.Extension,
            ["createdAt"] = media.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
            ["updatedAt"] = media.UpdatedAt.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
        };
    }
}
