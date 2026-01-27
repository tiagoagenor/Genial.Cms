using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Genial.Cms.Domain.Aggregates;
using Genial.Cms.Domain.SeedWork;
using Genial.Cms.Infra.Data.Context;
using MongoDB.Driver;

namespace Genial.Cms.Infra.Data.Repositories;

public class TypeFileRepository : ITypeFileRepository
{
    private readonly IMongoCollection<TypeFile> _collection;

    public TypeFileRepository(MongoDbContext mongoDbContext)
    {
        _collection = mongoDbContext.GetCollection<TypeFile>("_typeFiles");
    }

    public async Task<TypeFile> GetByKeyAsync(string key, CancellationToken cancellationToken = default)
    {
        return await _collection.Find(t => t.Key == key).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<TypeFile> GetByKeyAndValueAsync(string key, string value, CancellationToken cancellationToken = default)
    {
        return await _collection.Find(t => t.Key == key && t.Value == value).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<TypeFile> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _collection.Find(t => t.Id == id).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<TypeFile>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _collection.Find(_ => true).SortBy(t => t.Order).ToListAsync(cancellationToken);
    }

    public async Task InsertAsync(TypeFile typeFile, CancellationToken cancellationToken = default)
    {
        await _collection.InsertOneAsync(typeFile, cancellationToken: cancellationToken);
    }

    public async Task InsertManyAsync(List<TypeFile> typeFiles, CancellationToken cancellationToken = default)
    {
        if (typeFiles != null && typeFiles.Count > 0)
        {
            await _collection.InsertManyAsync(typeFiles, cancellationToken: cancellationToken);
        }
    }

    public async Task UpdateAsync(TypeFile typeFile, CancellationToken cancellationToken = default)
    {
        await _collection.ReplaceOneAsync(t => t.Id == typeFile.Id, typeFile, cancellationToken: cancellationToken);
    }

    public async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        await _collection.DeleteOneAsync(t => t.Id == id, cancellationToken);
    }
}
