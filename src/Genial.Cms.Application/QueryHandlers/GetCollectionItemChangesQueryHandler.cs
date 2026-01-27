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

public class GetCollectionItemChangesQueryHandler : IRequestHandler<GetCollectionItemChangesQuery, IEnumerable<GetCollectionItemChangesQueryResult>>
{
    private readonly ICollectionItemChangeRepository _collectionItemChangeRepository;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<GetCollectionItemChangesQueryHandler> _logger;

    public GetCollectionItemChangesQueryHandler(
        ICollectionItemChangeRepository collectionItemChangeRepository,
        IUserRepository userRepository,
        ILogger<GetCollectionItemChangesQueryHandler> logger)
    {
        _collectionItemChangeRepository = collectionItemChangeRepository;
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<GetCollectionItemChangesQueryResult>> Handle(GetCollectionItemChangesQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Buscando histórico de alterações. CollectionId: {CollectionId}, ItemId: {ItemId}", 
            request.CollectionId, request.ItemId);

        var changes = await _collectionItemChangeRepository.GetByCollectionIdAndItemIdAsync(
            request.CollectionId, 
            request.ItemId, 
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

        var results = changes.Select(change => new GetCollectionItemChangesQueryResult
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

        _logger.LogInformation("Encontradas {Count} alterações para item {ItemId}", results.Count, request.ItemId);

        return results;
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
