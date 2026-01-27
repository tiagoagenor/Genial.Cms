using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Genial.Cms.Domain.Aggregates;

namespace Genial.Cms.Domain.SeedWork;

public interface IMediaRepository
{
    Task<Media> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<Media> GetByUrlAsync(string url, CancellationToken cancellationToken = default);
    Task<Media> GetByFileNameUrlAsync(string fileNameUrl, CancellationToken cancellationToken = default);
    Task<List<Media>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<List<Media>> GetByTagsAsync(List<string> tags, CancellationToken cancellationToken = default);
    Task<(List<Media> Media, int Total)> GetPaginatedAsync(
        int page, 
        int pageSize, 
        List<string>? tags = null,
        string? contentType = null,
        string? extension = null,
        string? stageId = null,
        string sortBy = "createdAt",
        string sortDirection = "desc",
        CancellationToken cancellationToken = default);
    Task InsertAsync(Media media, CancellationToken cancellationToken = default);
    Task UpdateAsync(Media media, CancellationToken cancellationToken = default);
    Task DeleteAsync(string id, CancellationToken cancellationToken = default);
    Task<int> CountByStageIdAsync(string stageId, CancellationToken cancellationToken = default);
    Task<long> GetTotalFileSizeByStageIdAsync(string stageId, CancellationToken cancellationToken = default);
}
