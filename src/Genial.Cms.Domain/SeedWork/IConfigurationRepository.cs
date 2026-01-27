using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Genial.Cms.Domain.Aggregates;

namespace Genial.Cms.Domain.SeedWork;

public interface IConfigurationRepository
{
    Task<Configuration> GetByKeyAsync(string key, CancellationToken cancellationToken = default);
    Task<Configuration> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<List<Configuration>> GetAllAsync(CancellationToken cancellationToken = default);
    Task InsertAsync(Configuration configuration, CancellationToken cancellationToken = default);
    Task UpdateAsync(Configuration configuration, CancellationToken cancellationToken = default);
    Task DeleteAsync(string id, CancellationToken cancellationToken = default);
}
