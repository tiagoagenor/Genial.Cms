#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Genial.Cms.Application.Commands.Collection;
using Genial.Cms.Domain.Aggregates;
using Genial.Cms.Domain.Exceptions;
using Genial.Cms.Domain.SeedWork;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;

namespace Genial.Cms.Application.CommandHandlers.Collection;

public class GetCollectionItemCommandHandler : IRequestHandler<GetCollectionItemCommand, GetCollectionItemResponse>
{
    private readonly ICollectionRepository _collectionRepository;
    private readonly IMediaRepository _mediaRepository;
    private readonly IConfiguration _configuration;
    private readonly IMediator _bus;
    private readonly ILogger<GetCollectionItemCommandHandler> _logger;

    public GetCollectionItemCommandHandler(
        ICollectionRepository collectionRepository,
        IMediaRepository mediaRepository,
        IConfiguration configuration,
        IMediator bus,
        ILogger<GetCollectionItemCommandHandler> logger)
    {
        _collectionRepository = collectionRepository;
        _mediaRepository = mediaRepository;
        _configuration = configuration;
        _bus = bus;
        _logger = logger;
    }

    public async Task<GetCollectionItemResponse> Handle(GetCollectionItemCommand request, CancellationToken cancellationToken)
    {
        // Validar CollectionId
        if (string.IsNullOrWhiteSpace(request.CollectionId))
        {
            _logger.LogWarning("CollectionId não foi fornecido");
            await _bus.Publish(new ExceptionNotification("064", "CollectionId é obrigatório", ExceptionType.Client, "CollectionId"), cancellationToken);
            return null!;
        }

        // Validar ItemId
        if (string.IsNullOrWhiteSpace(request.ItemId))
        {
            _logger.LogWarning("ItemId não foi fornecido");
            await _bus.Publish(new ExceptionNotification("066", "ItemId é obrigatório", ExceptionType.Client, "ItemId"), cancellationToken);
            return null!;
        }

        _logger.LogInformation("Buscando item {ItemId} da collection {CollectionId}", request.ItemId, request.CollectionId);

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
        Console.WriteLine("\n\n\n\n--------------------------------");
        Console.WriteLine(collection.CollectionName);
        Console.WriteLine(request.ItemId);
        Console.WriteLine("--------------------------------\n\n\n\n");

        // Buscar o item específico na collection MongoDB dinâmica através do repository
        var item = await _collectionRepository.GetCollectionItemByIdAsync(collection.CollectionName, request.ItemId, cancellationToken);

        if (item == null)
        {
            _logger.LogWarning("Item não encontrado: {ItemId} na collection {CollectionName}", request.ItemId, collection.CollectionName);
            await _bus.Publish(new ExceptionNotification("067", "Item não encontrado", ExceptionType.Client, "ItemId"), cancellationToken);
            return null!;
        }

        // Converter BsonDocument para Dictionary
        var itemDictionary = ConvertBsonDocumentToDictionary(item);

        // Fazer join com _media para campos do tipo "file"
        if (collection.Fields != null && collection.Fields.Count > 0)
        {
            var fileFields = collection.Fields.Where(f =>
                !string.IsNullOrWhiteSpace(f.Type) &&
                f.Type.Equals("file", System.StringComparison.OrdinalIgnoreCase) &&
                !string.IsNullOrWhiteSpace(f.Slug)).ToList();

            foreach (var fileField in fileFields)
            {
                // Verificar se o campo existe no item e tem um valor
                if (!itemDictionary.ContainsKey(fileField.Slug) || itemDictionary[fileField.Slug] == null)
                    continue;

                var fieldValue = itemDictionary[fileField.Slug];

                // Verificar se já é um Dictionary com dados completos no formato correto
                if (fieldValue is Dictionary<string, object> existingDict)
                {
                    // Verificar se já está no formato completo e válido (com campos principais do Media)
                    if (existingDict.ContainsKey("_id") &&
                        existingDict.ContainsKey("fileName") &&
                        existingDict.ContainsKey("fileNameUrl") &&
                        existingDict.ContainsKey("contentType") &&
                        existingDict.ContainsKey("fileSize") &&
                        existingDict.ContainsKey("url"))
                    {
                        // Já está no formato correto com dados completos, manter como está
                        _logger.LogDebug("Campo {FieldSlug} já tem dados completos do media, mantendo como está", fileField.Slug);
                        continue;
                    }

                    // Se é um Dictionary mas não tem dados válidos, tentar extrair ID para buscar
                    if (!IsValidMediaDictionary(existingDict))
                    {
                        _logger.LogWarning("Campo {FieldSlug} tem um Dictionary inválido. Tentando extrair identificador ou limpar.", fileField.Slug);
                        // Continuar para tentar extrair identificador ou limpar
                    }
                }

                // Extrair identificador do campo
                var identifier = ExtractMediaIdentifier(fieldValue);

                if (string.IsNullOrWhiteSpace(identifier))
                {
                    // Não conseguiu extrair identificador válido
                    _logger.LogWarning("Não foi possível extrair identificador do campo {FieldSlug}. Tipo: {Type}",
                        fileField.Slug, fieldValue?.GetType().Name ?? "null");

                    // Se é um Dictionary inválido, limpar para evitar "[object Object]"
                    if (fieldValue is Dictionary<string, object> invalidDict && !IsValidMediaDictionary(invalidDict))
                    {
                        itemDictionary[fileField.Slug] = string.Empty;
                        _logger.LogWarning("Campo {FieldSlug} tinha um objeto inválido, foi limpo", fileField.Slug);
                    }
                    continue;
                }

                // Buscar o media no MongoDB
                try
                {
                    var media = await FindMediaAsync(identifier, fileField.Slug, cancellationToken);

                    if (media is not null)
                    {
                        // Converter Media para Dictionary no formato MongoDB e substituir
                        var mediaDictionary = ConvertMediaToMongoDictionary(media);
                        Console.WriteLine(
                            $"\n\n\n\n========== Media Dictionary ==========\n\n" +
                            JsonSerializer.Serialize(mediaDictionary, new JsonSerializerOptions
                            {
                                WriteIndented = true
                            }) +
                            "\n\n=====================================\n\n\n\n"
                        );
                        itemDictionary[fileField.Slug] = mediaDictionary;
                        _logger.LogInformation("Campo {FieldSlug} atualizado com dados do media: {MediaId}", fileField.Slug, media.Id);
                    }
                    else
                    {
                        _logger.LogWarning("Media não encontrado para o identificador: {Identifier} no campo {FieldSlug}",
                            identifier, fileField.Slug);

                        // Se não encontrou e o valor original era um Dictionary inválido, limpar
                        if (fieldValue is Dictionary<string, object> invalidDict && !IsValidMediaDictionary(invalidDict))
                        {
                            itemDictionary[fileField.Slug] = string.Empty;
                            _logger.LogWarning("Campo {FieldSlug} tinha um objeto inválido, foi limpo", fileField.Slug);
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    _logger.LogWarning(ex, "Erro ao buscar media para o identificador: {Identifier} no campo {FieldSlug}. Erro: {Error}",
                        identifier, fileField.Slug, ex.Message);

                    // Se houver erro e o valor original era um Dictionary inválido, limpar
                    if (fieldValue is Dictionary<string, object> errorDict && !IsValidMediaDictionary(errorDict))
                    {
                        itemDictionary[fileField.Slug] = string.Empty;
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

        _logger.LogInformation("Item encontrado: {ItemId} da collection {CollectionId}", request.ItemId, request.CollectionId);

        return new GetCollectionItemResponse
        {
            CollectionName = collection.Name,
            Columns = columns,
            Item = itemDictionary
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
    /// Converte um objeto Media para MediaDto no formato simplificado
    /// </summary>
    private MediaDto ConvertMediaToMongoDictionary(Media media)
    {
        return new MediaDto
        {
            FileId = media.Id,
            FileName = media.FileName,
            FileNameUrl = media.FileNameUrl ?? string.Empty,
            ContentType = media.ContentType,
            FileSize = media.FileSize,
            Url = media.Url,
            Tags = media.Tags ?? new List<string>(),
            Extension = media.Extension,
            StageId = media.StageId ?? string.Empty,
            CreatedAt = media.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
            UpdatedAt = media.UpdatedAt.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
        };
    }

    /// <summary>
    /// Extrai um identificador (ID, fileNameUrl ou URL) de um valor de campo do tipo file
    /// </summary>
    private string? ExtractMediaIdentifier(object? fieldValue)
    {
        if (fieldValue is null)
            return null;

        if (fieldValue is string stringValue)
            return stringValue;

        if (fieldValue is Dictionary<string, object> dictValue)
        {
            // Verificar se já tem dados completos no formato correto
            if (dictValue.ContainsKey("_id") &&
                dictValue.ContainsKey("fileName") &&
                dictValue.ContainsKey("fileNameUrl"))
            {
                // Já está completo, retornar o ID para validação
                var idValue = dictValue["_id"];
                // Se _id é um objeto com $oid, extrair o $oid
                if (idValue is Dictionary<string, object> idObj && idObj.ContainsKey("$oid"))
                    return idObj["$oid"]?.ToString();
                else
                    return idValue?.ToString();
            }

            // Extrair ID de diferentes formatos
            if (dictValue.ContainsKey("_id"))
            {
                var idValue = dictValue["_id"];
                // Se _id é um objeto com $oid, extrair o $oid
                if (idValue is Dictionary<string, object> idObj && idObj.ContainsKey("$oid"))
                    return idObj["$oid"]?.ToString();
                else
                    return idValue?.ToString();
            }

            if (dictValue.ContainsKey("id"))
                return dictValue["id"]?.ToString();

            if (dictValue.ContainsKey("fileNameUrl"))
                return dictValue["fileNameUrl"]?.ToString();

            if (dictValue.ContainsKey("url"))
                return dictValue["url"]?.ToString();
        }

        return null;
    }

    /// <summary>
    /// Busca um Media no MongoDB usando múltiplas estratégias
    /// </summary>
    private async Task<Media?> FindMediaAsync(string identifier, string fieldSlug, CancellationToken cancellationToken)
    {
        Media? media = null;

        // Estratégia 1: Tentar buscar pelo ID (ObjectId do MongoDB)
        if (MongoDB.Bson.ObjectId.TryParse(identifier, out _))
        {
            media = await _mediaRepository.GetByIdAsync(identifier, cancellationToken);
            if (media is not null)
            {
                _logger.LogInformation("Media encontrado pelo ID: {Value} no campo {FieldSlug}", identifier, fieldSlug);
                return media;
            }
        }

        // Estratégia 2: Se não encontrou, tentar buscar pela URL completa
        if (media is null && identifier.StartsWith("http", System.StringComparison.OrdinalIgnoreCase))
        {
            media = await _mediaRepository.GetByUrlAsync(identifier, cancellationToken);
            if (media is not null)
            {
                _logger.LogInformation("Media encontrado pela URL: {Value} no campo {FieldSlug}", identifier, fieldSlug);
                return media;
            }
        }

        // Estratégia 3: Se não encontrou, tentar buscar pelo fileNameUrl (UUID.extensão)
        if (media is null)
        {
            media = await _mediaRepository.GetByFileNameUrlAsync(identifier, cancellationToken);
            if (media is not null)
            {
                _logger.LogInformation("Media encontrado pelo fileNameUrl: {Value} no campo {FieldSlug}", identifier, fieldSlug);
                return media;
            }
        }

        // Estratégia 4: Se ainda não encontrou, tentar construir a URL e buscar
        if (media is null)
        {
            var baseUrl = _configuration["FileUpload:BaseUrl"] ?? "http://localhost:5000/v1/files/upload";
            var fullUrl = $"{baseUrl.TrimEnd('/')}/{identifier}";
            media = await _mediaRepository.GetByUrlAsync(fullUrl, cancellationToken);
            if (media is not null)
            {
                _logger.LogInformation("Media encontrado pela URL construída: {Url} no campo {FieldSlug}", fullUrl, fieldSlug);
                return media;
            }
        }

        return null;
    }

    /// <summary>
    /// Verifica se um Dictionary é um objeto Media válido e completo
    /// </summary>
    private bool IsValidMediaDictionary(Dictionary<string, object> dict)
    {
        return dict.ContainsKey("id") ||
               dict.ContainsKey("_id") ||
               dict.ContainsKey("fileNameUrl") ||
               dict.ContainsKey("url") ||
               dict.ContainsKey("fileName");
    }
}

/// <summary>
/// DTO para representar dados de Media no formato simplificado
/// </summary>
public class MediaDto
{
    public string FileId { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string FileNameUrl { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string Url { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = new();
    public string Extension { get; set; } = string.Empty;
    public string StageId { get; set; } = string.Empty;
    public string CreatedAt { get; set; } = string.Empty;
    public string UpdatedAt { get; set; } = string.Empty;
}
