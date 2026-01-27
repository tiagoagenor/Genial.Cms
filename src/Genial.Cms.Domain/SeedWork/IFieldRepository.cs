using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Genial.Cms.Domain.Aggregates;

namespace Genial.Cms.Domain.SeedWork;

public interface IFieldRepository
{
    Task<Field> GetByKeyAsync(string key, CancellationToken cancellationToken = default);
    Task<Field> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<List<Field>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<List<Field>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task InsertAsync(Field field, CancellationToken cancellationToken = default);
    Task UpdateAsync(Field field, CancellationToken cancellationToken = default);
    Task DeleteAsync(string id, CancellationToken cancellationToken = default);
}

