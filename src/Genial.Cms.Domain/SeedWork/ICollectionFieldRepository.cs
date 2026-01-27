using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Genial.Cms.Domain.Aggregates;

namespace Genial.Cms.Domain.SeedWork;

public interface ICollectionFieldRepository
{
    Task<CollectionField> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<CollectionField> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<CollectionField> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<List<CollectionField>> GetAllAsync(CancellationToken cancellationToken = default);
    Task InsertAsync(CollectionField field, CancellationToken cancellationToken = default);
    Task InsertManyAsync(List<CollectionField> fields, CancellationToken cancellationToken = default);
    Task UpdateAsync(CollectionField field, CancellationToken cancellationToken = default);
    Task DeleteAsync(string id, CancellationToken cancellationToken = default);
}
