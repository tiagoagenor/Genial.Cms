using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Genial.Cms.Domain.Aggregates;

namespace Genial.Cms.Domain.SeedWork;

public interface IStageRepository
{
    Task<Stage> GetByKeyAsync(string key, CancellationToken cancellationToken = default);
    Task<Stage> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<Stage> GetFirstAsync(CancellationToken cancellationToken = default);
    Task<List<Stage>> GetAllAsync(CancellationToken cancellationToken = default);
    Task InsertAsync(Stage stage, CancellationToken cancellationToken = default);
}

