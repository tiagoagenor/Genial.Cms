using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Genial.Cms.Domain.Aggregates;
using Genial.Cms.Domain.SeedWork;
using Genial.Cms.Infra.Data.Context;
using MongoDB.Driver;

namespace Genial.Cms.Infra.Data.Repositories;

public class FieldRepository : IFieldRepository
{
    private readonly IMongoCollection<Field> _collection;

    public FieldRepository(MongoDbContext mongoDbContext)
    {
        _collection = mongoDbContext.GetCollection<Field>("_field");
    }

    public async Task<Field> GetByKeyAsync(string key, CancellationToken cancellationToken = default)
    {
        return await _collection.Find(f => f.Key == key).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Field> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _collection.Find(f => f.Id == id).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<Field>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _collection.Find(_ => true).SortBy(f => f.Order).ToListAsync(cancellationToken);
    }

    public async Task<List<Field>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _collection.Find(f => f.Active).SortBy(f => f.Order).ToListAsync(cancellationToken);
    }

    public async Task InsertAsync(Field field, CancellationToken cancellationToken = default)
    {
        await _collection.InsertOneAsync(field, cancellationToken: cancellationToken);
    }

    public async Task UpdateAsync(Field field, CancellationToken cancellationToken = default)
    {
        await _collection.ReplaceOneAsync(f => f.Id == field.Id, field, cancellationToken: cancellationToken);
    }

    public async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        await _collection.DeleteOneAsync(f => f.Id == id, cancellationToken);
    }
}

