using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Genial.Cms.Domain.Aggregates;
using Genial.Cms.Domain.SeedWork;
using Genial.Cms.Infra.Data.Context;
using MongoDB.Driver;

namespace Genial.Cms.Infra.Data.Repositories;

public class StageRepository : IStageRepository
{
    private readonly IMongoCollection<Stage> _collection;

    public StageRepository(MongoDbContext mongoDbContext)
    {
        _collection = mongoDbContext.GetCollection<Stage>("_stage");
    }

    public async Task<Stage> GetByKeyAsync(string key, CancellationToken cancellationToken = default)
    {
        return await _collection.Find(s => s.Key == key).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Stage> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _collection.Find(s => s.Id == id).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Stage> GetFirstAsync(CancellationToken cancellationToken = default)
    {
        return await _collection.Find(s => s.Active).SortBy(s => s.Order).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<Stage>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _collection.Find(_ => true).SortBy(s => s.Order).ToListAsync(cancellationToken);
    }

    public async Task InsertAsync(Stage stage, CancellationToken cancellationToken = default)
    {
        await _collection.InsertOneAsync(stage, cancellationToken: cancellationToken);
    }
}

