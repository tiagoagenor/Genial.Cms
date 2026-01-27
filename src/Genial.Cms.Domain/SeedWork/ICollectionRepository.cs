using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Genial.Cms.Domain.Aggregates;
using MongoDB.Bson;

namespace Genial.Cms.Domain.SeedWork;

public interface ICollectionRepository
{
    Task<Collection> GetByKeyAsync(string key, CancellationToken cancellationToken = default);
    Task<Collection> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<Collection> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<Collection> GetByNameAndStageIdAsync(string name, string stageId, CancellationToken cancellationToken = default);
    Task<Collection> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<Collection> GetBySlugAndStageIdAsync(string slug, string stageId, CancellationToken cancellationToken = default);
    Task<Collection> GetByCollectionNameAsync(string collectionName, CancellationToken cancellationToken = default);
    Task<Collection> GetByCollectionNameAndStageIdAsync(string collectionName, string stageId, CancellationToken cancellationToken = default);
    Task<List<Collection>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<List<Collection>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<List<Collection>> GetByStageIdAsync(string stageId, CancellationToken cancellationToken = default);
    Task<(List<Collection> Collections, int Total)> GetByStageIdPaginatedAsync(string stageId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task InsertAsync(Collection collection, CancellationToken cancellationToken = default);
    Task UpdateAsync(Collection collection, CancellationToken cancellationToken = default);
    Task DeleteAsync(string id, CancellationToken cancellationToken = default);
    Task<bool> CreateMongoCollectionAsync(string collectionName, CancellationToken cancellationToken = default);
    Task<BsonDocument?> GetCollectionItemByIdAsync(string collectionName, string itemId, CancellationToken cancellationToken = default);
    Task<int> CountByStageIdAsync(string stageId, CancellationToken cancellationToken = default);
}
