using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Genial.Cms.Application.Queries;
using Genial.Cms.Domain.SeedWork;
using MediatR;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;

namespace Genial.Cms.Application.QueryHandlers;

public class GetCollectionChangesQueryHandler : IRequestHandler<GetCollectionChangesQuery, GetCollectionChangesQueryResult>
{
    private readonly ICollectionItemChangeRepository _collectionItemChangeRepository;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<GetCollectionChangesQueryHandler> _logger;

    public GetCollectionChangesQueryHandler(
        ICollectionItemChangeRepository collectionItemChangeRepository,
        IUserRepository userRepository,
        ILogger<GetCollectionChangesQueryHandler> logger)
    {
        _collectionItemChangeRepository = collectionItemChangeRepository;
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<GetCollectionChangesQueryResult> Handle(GetCollectionChangesQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Buscando histórico de alterações da collection. CollectionId: {CollectionId}, Page: {Page}, PageSize: {PageSize}", 
            request.CollectionId, request.Page, request.PageSize);

        var (changes, total) = await _collectionItemChangeRepository.GetByCollectionIdPaginatedAsync(
            request.CollectionId,
            request.Page,
            request.PageSize,
            cancellationToken);

        // Buscar informações dos usuários de forma eficiente
        var userIds = changes.Select(c => c.UserId).Distinct().ToList();
        var usersDict = new Dictionary<string, UserInfo>();
        
        foreach (var userId in userIds)
        {
            if (!string.IsNullOrWhiteSpace(userId))
            {
                try
                {
                    var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
                    if (user != null)
                    {
                        usersDict[userId] = new UserInfo
                        {
                            Id = user.Id,
                            Email = user.Email ?? string.Empty
                        };
                    }
                }
                catch (System.Exception ex)
                {
                    _logger.LogWarning(ex, "Erro ao buscar usuário {UserId}", userId);
                }
            }
        }

        var data = changes.Select(change => new GetCollectionItemChangesQueryResult
        {
            Id = change.Id,
            CollectionId = change.CollectionId,
            ItemId = change.ItemId,
            User = usersDict.TryGetValue(change.UserId, out var userInfo) ? userInfo : new UserInfo { Id = change.UserId },
            ChangeType = change.ChangeType,
            BeforeData = change.BeforeData != null ? ConvertBsonDocumentToObject(change.BeforeData) : null,
            AfterData = change.AfterData != null ? ConvertBsonDocumentToObject(change.AfterData) : null,
            CreatedAt = change.CreatedAt
        }).ToList();

        var totalPages = (int)Math.Ceiling(total / (double)request.PageSize);

        _logger.LogInformation("Encontradas {Count} alterações de {Total} para collection {CollectionId} (página {Page} de {TotalPages})", 
            data.Count, total, request.CollectionId, request.Page, totalPages);

        return new GetCollectionChangesQueryResult
        {
            Data = data,
            Total = total,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalPages = totalPages
        };
    }

    private object? ConvertBsonDocumentToObject(BsonDocument document)
    {
        try
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
        catch (System.Exception ex)
        {
            _logger.LogWarning(ex, "Erro ao converter BsonDocument para objeto");
            return null;
        }
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
