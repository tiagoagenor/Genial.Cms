using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Genial.Cms.Domain.Aggregates;
using Genial.Cms.Domain.SeedWork;
using Genial.Cms.Infra.Data.Context;
using MongoDB.Driver;

namespace Genial.Cms.Infra.Data.Repositories;

public class CollectionFieldRepository : ICollectionFieldRepository
{
    private readonly IMongoCollection<CollectionField> _collection;

    public CollectionFieldRepository(MongoDbContext mongoDbContext)
    {
        _collection = mongoDbContext.GetCollection<CollectionField>("_collection_field");
    }

    public async Task<CollectionField> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _collection.Find(f => f.Id == id).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<CollectionField> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await _collection.Find(f => f.Slug == slug).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<CollectionField> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _collection.Find(f => f.Name == name).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<CollectionField>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _collection.Find(_ => true).SortBy(f => f.CreatedAt).ToListAsync(cancellationToken);
    }

    public async Task InsertAsync(CollectionField field, CancellationToken cancellationToken = default)
    {
        await _collection.InsertOneAsync(field, cancellationToken: cancellationToken);
    }

    public async Task InsertManyAsync(List<CollectionField> fields, CancellationToken cancellationToken = default)
    {
        if (fields != null && fields.Count > 0)
        {
            await _collection.InsertManyAsync(fields, cancellationToken: cancellationToken);
        }
    }

    public async Task UpdateAsync(CollectionField field, CancellationToken cancellationToken = default)
    {
        await _collection.ReplaceOneAsync(f => f.Id == field.Id, field, cancellationToken: cancellationToken);
    }

    public async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        await _collection.DeleteOneAsync(f => f.Id == id, cancellationToken);
    }
}
