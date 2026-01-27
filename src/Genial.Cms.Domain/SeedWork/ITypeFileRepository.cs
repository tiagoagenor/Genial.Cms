using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Genial.Cms.Domain.Aggregates;

namespace Genial.Cms.Domain.SeedWork;

public interface ITypeFileRepository
{
    Task<TypeFile> GetByKeyAsync(string key, CancellationToken cancellationToken = default);
    Task<TypeFile> GetByKeyAndValueAsync(string key, string value, CancellationToken cancellationToken = default);
    Task<TypeFile> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<List<TypeFile>> GetAllAsync(CancellationToken cancellationToken = default);
    Task InsertAsync(TypeFile typeFile, CancellationToken cancellationToken = default);
    Task InsertManyAsync(List<TypeFile> typeFiles, CancellationToken cancellationToken = default);
    Task UpdateAsync(TypeFile typeFile, CancellationToken cancellationToken = default);
    Task DeleteAsync(string id, CancellationToken cancellationToken = default);
}
