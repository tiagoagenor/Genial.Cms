using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Genial.Cms.Domain.Aggregates;
using Genial.Cms.Domain.SeedWork;
using Genial.Cms.Infra.Data.Context;
using MongoDB.Driver;

namespace Genial.Cms.Infra.Data.Repositories;

public class ConfigurationRepository : IConfigurationRepository
{
    private readonly IMongoCollection<Configuration> _collection;

    public ConfigurationRepository(MongoDbContext mongoDbContext)
    {
        _collection = mongoDbContext.GetCollection<Configuration>("_config");
    }

    public async Task<Configuration> GetByKeyAsync(string key, CancellationToken cancellationToken = default)
    {
        return await _collection.Find(c => c.Key == key).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Configuration> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _collection.Find(c => c.Id == id).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<Configuration>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _collection.Find(_ => true).SortBy(c => c.Key).ToListAsync(cancellationToken);
    }

    public async Task InsertAsync(Configuration configuration, CancellationToken cancellationToken = default)
    {
        await _collection.InsertOneAsync(configuration, cancellationToken: cancellationToken);
    }

    public async Task UpdateAsync(Configuration configuration, CancellationToken cancellationToken = default)
    {
        await _collection.ReplaceOneAsync(c => c.Id == configuration.Id, configuration, cancellationToken: cancellationToken);
    }

    public async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        await _collection.DeleteOneAsync(c => c.Id == id, cancellationToken);
    }
}
