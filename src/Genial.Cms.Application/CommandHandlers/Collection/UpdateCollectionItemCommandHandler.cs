#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Genial.Cms.Application.Commands.Collection;
using CollectionAggregate = Genial.Cms.Domain.Aggregates.Collection;
using Genial.Cms.Domain.Aggregates;
using Genial.Cms.Domain.Exceptions;
using Genial.Cms.Domain.SeedWork;
using Genial.Cms.Infra.Data.Context;
using MediatR;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Genial.Cms.Application.CommandHandlers.Collection;

public class UpdateCollectionItemCommandHandler : IRequestHandler<UpdateCollectionItemCommand, UpdateCollectionItemResponse>
{
    private readonly ICollectionRepository _collectionRepository;
    private readonly MongoDbContext _mongoDbContext;
    private readonly IMediator _bus;
    private readonly ILogger<UpdateCollectionItemCommandHandler> _logger;
    private readonly ICollectionItemChangeRepository _collectionItemChangeRepository;

    public UpdateCollectionItemCommandHandler(
        ICollectionRepository collectionRepository,
        MongoDbContext mongoDbContext,
        IMediator bus,
        ILogger<UpdateCollectionItemCommandHandler> logger,
        ICollectionItemChangeRepository collectionItemChangeRepository)
    {
        _collectionRepository = collectionRepository;
        _mongoDbContext = mongoDbContext;
        _bus = bus;
        _logger = logger;
        _collectionItemChangeRepository = collectionItemChangeRepository;
    }

    public async Task<UpdateCollectionItemResponse> Handle(UpdateCollectionItemCommand request, CancellationToken cancellationToken)
    {
        // Validar CollectionId e ItemId
        if (string.IsNullOrWhiteSpace(request.CollectionId))
        {
            _logger.LogWarning("CollectionId não foi fornecido");
            await _bus.Publish(new ExceptionNotification("064", "CollectionId é obrigatório", ExceptionType.Client, "CollectionId"), cancellationToken);
            return null!;
        }

        if (string.IsNullOrWhiteSpace(request.ItemId))
        {
            _logger.LogWarning("ItemId não foi fornecido");
            await _bus.Publish(new ExceptionNotification("066", "ItemId é obrigatório", ExceptionType.Client, "ItemId"), cancellationToken);
            return null!;
        }

        _logger.LogInformation("Atualizando item {ItemId} da collection {CollectionId}", request.ItemId, request.CollectionId);

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

        // Validar dados baseado nos campos da collection
        var validationErrors = await ValidateDataAsync(collection, request.Data, cancellationToken);
        if (validationErrors.Count > 0)
        {
            _logger.LogWarning("Validação falhou para collection {CollectionId}: {Count} erro(s)", request.CollectionId, validationErrors.Count);
            
            foreach (var (fieldSlug, message) in validationErrors)
            {
                await _bus.Publish(new ExceptionNotification("063", message, ExceptionType.Client, fieldSlug), cancellationToken);
            }
            
            return null!;
        }

        // Buscar o item existente
        var mongoCollection = _mongoDbContext.Database.GetCollection<BsonDocument>(collection.CollectionName);
        var objectId = ObjectId.Parse(request.ItemId);
        var filter = Builders<BsonDocument>.Filter.Eq("_id", objectId);
        var existingItem = await mongoCollection.Find(filter).FirstOrDefaultAsync(cancellationToken);

        if (existingItem == null)
        {
            _logger.LogWarning("Item não encontrado: {ItemId} na collection {CollectionName}", request.ItemId, collection.CollectionName);
            await _bus.Publish(new ExceptionNotification("067", "Item não encontrado", ExceptionType.Client, "ItemId"), cancellationToken);
            return null!;
        }

        // Preparar documento para atualização
        var updateDefinition = new List<UpdateDefinition<BsonDocument>>();
        
        foreach (var field in collection.Fields)
        {
            var fieldSlug = field.Slug.ToLowerInvariant();
            if (request.Data.TryGetValue(fieldSlug, out var value))
            {
                var processedValue = ProcessFieldValue(field, value);
                var bsonValue = ConvertToBsonValue(processedValue);
                updateDefinition.Add(Builders<BsonDocument>.Update.Set(fieldSlug, bsonValue));
            }
        }

        // Atualizar updatedAt
        updateDefinition.Add(Builders<BsonDocument>.Update.Set("updatedAt", DateTime.UtcNow));

        // Aplicar atualizações
        var combinedUpdate = Builders<BsonDocument>.Update.Combine(updateDefinition);
        var updateResult = await mongoCollection.UpdateOneAsync(filter, combinedUpdate, cancellationToken: cancellationToken);

        if (updateResult.ModifiedCount == 0)
        {
            _logger.LogWarning("Nenhum item foi atualizado: {ItemId}", request.ItemId);
        }
        else
        {
            _logger.LogInformation("Item atualizado com sucesso: {ItemId} na collection {CollectionName}", request.ItemId, collection.CollectionName);
        }

        // Buscar o item atualizado
        var updatedItem = await mongoCollection.Find(filter).FirstOrDefaultAsync(cancellationToken);

        // Salvar histórico de alteração (beforeData = documento antes, afterData = documento depois)
        if (request.UserData != null && !string.IsNullOrWhiteSpace(request.UserData.UserId) && updatedItem != null)
        {
            try
            {
                var change = new CollectionItemChange
                {
                    CollectionId = request.CollectionId,
                    ItemId = request.ItemId,
                    UserId = request.UserData.UserId,
                    ChangeType = CollectionItemChangeTypeEnum.Edit.Id,
                    BeforeData = existingItem, // Dados antes da alteração
                    AfterData = updatedItem, // Dados após a alteração
                    CreatedAt = DateTime.UtcNow
                };

                await _collectionItemChangeRepository.InsertAsync(change, cancellationToken);
                _logger.LogInformation("Histórico de alteração salvo para item {ItemId}", request.ItemId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Erro ao salvar histórico de alteração. Continuando...");
                // Não falhar a operação se houver erro ao salvar histórico
            }
        }

        // Preparar resposta
        var responseData = new Dictionary<string, object>();
        foreach (var element in updatedItem.Elements)
        {
            if (element.Name == "_id")
            {
                responseData["id"] = element.Value.ToString();
            }
            else
            {
                responseData[element.Name] = ConvertBsonValueToObject(element.Value);
            }
        }

        return new UpdateCollectionItemResponse
        {
            Id = updatedItem["_id"].ToString(),
            Data = responseData
        };
    }

    // Reutilizar métodos do CreateCollectionItemCommandHandler
    private object ProcessFieldValue(CollectionField field, object value)
    {
        var fieldType = field.Type.ToLowerInvariant();
        
        if (value is Dictionary<string, object> dict)
        {
            if (dict.TryGetValue("tipo", out var tipoObj) || dict.TryGetValue("type", out tipoObj))
            {
                if (dict.TryGetValue("valor", out var valorObj) || dict.TryGetValue("value", out valorObj))
                {
                    return valorObj;
                }
                
                if (dict.TryGetValue("opcoes", out var opcoesObj) || dict.TryGetValue("options", out opcoesObj))
                {
                    return opcoesObj;
                }
            }
        }
        
        return value;
    }

    private async Task<List<(string FieldSlug, string Message)>> ValidateDataAsync(CollectionAggregate collection, Dictionary<string, object> data, CancellationToken cancellationToken)
    {
        var errors = new List<(string FieldSlug, string Message)>();

        foreach (var field in collection.Fields)
        {
            var fieldSlug = field.Slug.ToLowerInvariant();
            var fieldData = field.Data;

            var hasValue = data.TryGetValue(fieldSlug, out var value);
            
            object actualValue = value;
            if (value is Dictionary<string, object> dict)
            {
                if (dict.TryGetValue("valor", out var valorObj) || dict.TryGetValue("value", out valorObj))
                {
                    actualValue = valorObj;
                }
                else if (dict.TryGetValue("opcoes", out var opcoesObj) || dict.TryGetValue("options", out opcoesObj))
                {
                    actualValue = opcoesObj;
                }
            }

            if (fieldData.TryGetValue("required", out var requiredBson) && requiredBson.AsBoolean)
            {
                if (!hasValue || (actualValue == null) || (actualValue is string str && string.IsNullOrWhiteSpace(str)))
                {
                    errors.Add((fieldSlug, $"Campo '{field.Name}' é obrigatório"));
                    continue;
                }
            }

            if (!hasValue || actualValue == null)
            {
                continue;
            }

            if (fieldData.TryGetValue("minLength", out var minLengthBson) && !minLengthBson.IsBsonNull)
            {
                var minLength = minLengthBson.AsInt32;
                if (actualValue is string strValue && strValue.Length < minLength)
                {
                    errors.Add((fieldSlug, $"Campo '{field.Name}' deve ter no mínimo {minLength} caracteres"));
                }
            }

            if (fieldData.TryGetValue("maxLength", out var maxLengthBson) && !maxLengthBson.IsBsonNull)
            {
                var maxLength = maxLengthBson.AsInt32;
                if (actualValue is string strValue && strValue.Length > maxLength)
                {
                    errors.Add((fieldSlug, $"Campo '{field.Name}' deve ter no máximo {maxLength} caracteres"));
                }
            }

            if (fieldData.TryGetValue("validation", out var validationBson) && !validationBson.IsBsonNull)
            {
                var regexPattern = validationBson.AsString;
                if (!string.IsNullOrWhiteSpace(regexPattern) && actualValue is string strValue)
                {
                    try
                    {
                        if (!Regex.IsMatch(strValue, regexPattern))
                        {
                            errors.Add((fieldSlug, $"Campo '{field.Name}' não corresponde ao padrão de validação"));
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Erro ao validar regex para campo {FieldName}: {Pattern}", field.Name, regexPattern);
                        errors.Add((fieldSlug, $"Erro na validação do campo '{field.Name}': padrão inválido"));
                    }
                }
            }

            var fieldType = field.Type.ToLowerInvariant();
            if (fieldType == "select" || fieldType == "checkbox")
            {
                if (fieldData.TryGetValue("options", out var optionsBson) && !optionsBson.IsBsonNull && optionsBson.IsBsonArray)
                {
                    var optionsArray = optionsBson.AsBsonArray;
                    var validValues = new List<string>();

                    foreach (var option in optionsArray)
                    {
                        if (option.IsBsonDocument)
                        {
                            var optionDoc = option.AsBsonDocument;
                            if (optionDoc.TryGetValue("value", out var valueBson))
                            {
                                validValues.Add(valueBson.AsString);
                            }
                        }
                    }

                    if (validValues.Count > 0)
                    {
                        if (actualValue is System.Collections.IList listValue)
                        {
                            foreach (var item in listValue)
                            {
                                var itemStr = item?.ToString() ?? string.Empty;
                                if (!validValues.Contains(itemStr))
                                {
                                    errors.Add((fieldSlug, $"Campo '{field.Name}' contém valor inválido '{itemStr}'. Valores permitidos: {string.Join(", ", validValues)}"));
                                }
                            }
                        }
                        else if (actualValue is string strValue)
                        {
                            if (!validValues.Contains(strValue))
                            {
                                errors.Add((fieldSlug, $"Campo '{field.Name}' deve ter um valor válido. Valores permitidos: {string.Join(", ", validValues)}"));
                            }
                        }
                    }
                }
            }
            else if (fieldType == "radio")
            {
                // Radio tem options como array de objetos {label, value}
                if (fieldData.TryGetValue("options", out var optionsBson) && !optionsBson.IsBsonNull && optionsBson.IsBsonArray)
                {
                    var optionsArray = optionsBson.AsBsonArray;
                    var validValues = new List<string>();

                    foreach (var option in optionsArray)
                    {
                        if (option.IsBsonDocument)
                        {
                            var optionDoc = option.AsBsonDocument;
                            if (optionDoc.TryGetValue("value", out var valueBson))
                            {
                                validValues.Add(valueBson.AsString);
                            }
                        }
                    }

                    if (validValues.Count > 0 && actualValue is string strValue)
                    {
                        if (!validValues.Contains(strValue))
                        {
                            errors.Add((fieldSlug, $"Campo '{field.Name}' deve ter um valor válido. Valores permitidos: {string.Join(", ", validValues)}"));
                        }
                    }
                }
            }
            else if (fieldType == "range")
            {
                // Range tem min e max como valores decimais
                if (fieldData.TryGetValue("min", out var minBson) && !minBson.IsBsonNull &&
                    fieldData.TryGetValue("max", out var maxBson) && !maxBson.IsBsonNull)
                {
                    decimal min = minBson.BsonType switch
                    {
                        BsonType.Decimal128 => (decimal)minBson.AsDecimal128,
                        BsonType.Double => (decimal)minBson.AsDouble,
                        BsonType.Int32 => minBson.AsInt32,
                        BsonType.Int64 => minBson.AsInt64,
                        _ => 0
                    };
                    
                    decimal max = maxBson.BsonType switch
                    {
                        BsonType.Decimal128 => (decimal)maxBson.AsDecimal128,
                        BsonType.Double => (decimal)maxBson.AsDouble,
                        BsonType.Int32 => maxBson.AsInt32,
                        BsonType.Int64 => maxBson.AsInt64,
                        _ => 0
                    };

                    // Tentar converter o valor para decimal
                    decimal? numericValue = null;
                    if (actualValue is decimal decValue)
                    {
                        numericValue = decValue;
                    }
                    else if (actualValue is double doubleValue)
                    {
                        numericValue = (decimal)doubleValue;
                    }
                    else if (actualValue is int intValue)
                    {
                        numericValue = intValue;
                    }
                    else if (actualValue is long longValue)
                    {
                        numericValue = longValue;
                    }
                    else if (actualValue is JsonElement jsonElement)
                    {
                        // Tratar JsonElement que pode conter números
                        if (jsonElement.ValueKind == JsonValueKind.Number)
                        {
                            if (jsonElement.TryGetDecimal(out var jsonDecimal))
                            {
                                numericValue = jsonDecimal;
                            }
                            else if (jsonElement.TryGetInt32(out var jsonInt))
                            {
                                numericValue = jsonInt;
                            }
                            else if (jsonElement.TryGetInt64(out var jsonLong))
                            {
                                numericValue = jsonLong;
                            }
                            else if (jsonElement.TryGetDouble(out var jsonDouble))
                            {
                                numericValue = (decimal)jsonDouble;
                            }
                        }
                    }
                    else if (actualValue is string strValue && decimal.TryParse(strValue, out var parsedValue))
                    {
                        numericValue = parsedValue;
                    }
                    else if (actualValue != null)
                    {
                        // Última tentativa: converter via ToString e parse
                        var strValueFallback = actualValue.ToString();
                        if (decimal.TryParse(strValueFallback, out var parsedValueFallback))
                        {
                            numericValue = parsedValueFallback;
                        }
                    }

                    if (numericValue.HasValue)
                    {
                        if (numericValue.Value < min || numericValue.Value > max)
                        {
                            errors.Add((fieldSlug, $"Campo '{field.Name}' deve estar entre {min} e {max}"));
                        }
                    }
                    else
                    {
                        errors.Add((fieldSlug, $"Campo '{field.Name}' deve ser um valor numérico"));
                    }
                }
            }
            else if (fieldType == "color")
            {
                // Validar se o valor é um hexadecimal válido
                if (actualValue != null)
                {
                    var colorValue = actualValue.ToString()?.Trim() ?? string.Empty;
                    
                    if (!string.IsNullOrWhiteSpace(colorValue))
                    {
                        // Remover # se existir
                        if (colorValue.StartsWith("#"))
                        {
                            colorValue = colorValue.Substring(1);
                        }
                        
                        // Validar formato hexadecimal: 3 ou 6 caracteres hexadecimais
                        if (colorValue.Length != 3 && colorValue.Length != 6)
                        {
                            errors.Add((fieldSlug, $"Campo '{field.Name}' deve ser um valor hexadecimal válido (formato: #RRGGBB ou #RGB)"));
                        }
                        else if (!Regex.IsMatch(colorValue, @"^[0-9A-Fa-f]+$"))
                        {
                            errors.Add((fieldSlug, $"Campo '{field.Name}' deve ser um valor hexadecimal válido (formato: #RRGGBB ou #RGB)"));
                        }
                    }
                }
            }
        }

        return errors;
    }

    private BsonValue ConvertToBsonValue(object value)
    {
        return value switch
        {
            null => BsonNull.Value,
            string str => new BsonString(str),
            int i => new BsonInt32(i),
            long l => new BsonInt64(l),
            double d => new BsonDouble(d),
            decimal dec => new BsonDecimal128(dec),
            bool b => new BsonBoolean(b),
            DateTime dt => new BsonDateTime(dt),
            BsonValue bson => bson,
            JsonElement jsonElement => ConvertJsonElementToBsonValue(jsonElement),
            System.Collections.IList list => ConvertListToBsonArray(list),
            Dictionary<string, object> dict => ConvertDictionaryToBsonDocument(dict),
            _ => BsonValue.Create(value)
        };
    }

    private BsonValue ConvertJsonElementToBsonValue(JsonElement jsonElement)
    {
        return jsonElement.ValueKind switch
        {
            JsonValueKind.String => new BsonString(jsonElement.GetString() ?? string.Empty),
            JsonValueKind.Number => jsonElement.TryGetInt32(out var intValue) 
                ? new BsonInt32(intValue) 
                : jsonElement.TryGetInt64(out var longValue)
                    ? new BsonInt64(longValue)
                    : new BsonDouble(jsonElement.GetDouble()),
            JsonValueKind.True => new BsonBoolean(true),
            JsonValueKind.False => new BsonBoolean(false),
            JsonValueKind.Null => BsonNull.Value,
            JsonValueKind.Array => ConvertJsonArrayToBsonArray(jsonElement),
            JsonValueKind.Object => ConvertJsonObjectToBsonDocument(jsonElement),
            _ => BsonNull.Value
        };
    }

    private BsonArray ConvertJsonArrayToBsonArray(JsonElement jsonElement)
    {
        var array = new BsonArray();
        foreach (var element in jsonElement.EnumerateArray())
        {
            array.Add(ConvertJsonElementToBsonValue(element));
        }
        return array;
    }

    private BsonDocument ConvertJsonObjectToBsonDocument(JsonElement jsonElement)
    {
        var document = new BsonDocument();
        foreach (var property in jsonElement.EnumerateObject())
        {
            document[property.Name] = ConvertJsonElementToBsonValue(property.Value);
        }
        return document;
    }

    private BsonArray ConvertListToBsonArray(System.Collections.IList list)
    {
        var array = new BsonArray();
        foreach (var item in list)
        {
            array.Add(ConvertToBsonValue(item));
        }
        return array;
    }

    private BsonDocument ConvertDictionaryToBsonDocument(Dictionary<string, object> dict)
    {
        var document = new BsonDocument();
        foreach (var kvp in dict)
        {
            document[kvp.Key] = ConvertToBsonValue(kvp.Value);
        }
        return document;
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
