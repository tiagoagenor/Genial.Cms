using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Genial.Cms.Domain.Aggregates;

namespace Genial.Cms.Domain.SeedWork;

public interface ICollectionItemChangeRepository
{
    Task<List<CollectionItemChange>> GetByCollectionIdAndItemIdAsync(string collectionId, string itemId, CancellationToken cancellationToken = default);
    Task<List<CollectionItemChange>> GetByCollectionIdAsync(string collectionId, CancellationToken cancellationToken = default);
    Task<(List<CollectionItemChange> Changes, int Total)> GetByCollectionIdPaginatedAsync(string collectionId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task InsertAsync(CollectionItemChange change, CancellationToken cancellationToken = default);
}
