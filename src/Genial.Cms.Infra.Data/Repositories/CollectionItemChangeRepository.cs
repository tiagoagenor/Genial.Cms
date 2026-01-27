using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Genial.Cms.Domain.Aggregates;
using Genial.Cms.Domain.SeedWork;
using Genial.Cms.Infra.Data.Context;
using MongoDB.Driver;

namespace Genial.Cms.Infra.Data.Repositories;

public class CollectionItemChangeRepository : ICollectionItemChangeRepository
{
    private readonly IMongoCollection<CollectionItemChange> _collection;

    public CollectionItemChangeRepository(MongoDbContext mongoDbContext)
    {
        _collection = mongoDbContext.GetCollection<CollectionItemChange>("_collectionItemChange");
    }

    public async Task<List<CollectionItemChange>> GetByCollectionIdAndItemIdAsync(string collectionId, string itemId, CancellationToken cancellationToken = default)
    {
        var filter = Builders<CollectionItemChange>.Filter.And(
            Builders<CollectionItemChange>.Filter.Eq(c => c.CollectionId, collectionId),
            Builders<CollectionItemChange>.Filter.Eq(c => c.ItemId, itemId)
        );

        return await _collection
            .Find(filter)
            .SortByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<CollectionItemChange>> GetByCollectionIdAsync(string collectionId, CancellationToken cancellationToken = default)
    {
        var filter = Builders<CollectionItemChange>.Filter.Eq(c => c.CollectionId, collectionId);

        return await _collection
            .Find(filter)
            .SortByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<(List<CollectionItemChange> Changes, int Total)> GetByCollectionIdPaginatedAsync(string collectionId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var filter = Builders<CollectionItemChange>.Filter.Eq(c => c.CollectionId, collectionId);

        // Contar total de documentos
        var total = (int)await _collection.CountDocumentsAsync(filter, cancellationToken: cancellationToken);

        // Aplicar paginação
        var skip = (page - 1) * pageSize;
        var changes = await _collection
            .Find(filter)
            .SortByDescending(c => c.CreatedAt)
            .Skip(skip)
            .Limit(pageSize)
            .ToListAsync(cancellationToken);

        return (changes, total);
    }

    public async Task InsertAsync(CollectionItemChange change, CancellationToken cancellationToken = default)
    {
        await _collection.InsertOneAsync(change, cancellationToken: cancellationToken);
    }
}
