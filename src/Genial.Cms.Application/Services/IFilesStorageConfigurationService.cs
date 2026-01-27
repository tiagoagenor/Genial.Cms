using System.Threading;
using System.Threading.Tasks;

namespace Genial.Cms.Application.Services;

public interface IFilesStorageConfigurationService
{
    Task<FilesStorageConfiguration> GetConfigurationAsync(CancellationToken cancellationToken = default);
    bool IsEnabled();
    Task<bool> TestConnectionAsync(CancellationToken cancellationToken = default);
}

public class FilesStorageConfiguration
{
    public bool Status { get; set; }
    public string Bucket { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public string? Endpoint { get; set; }
    public string AccessKeyId { get; set; } = string.Empty;
    public string SecretAccessKey { get; set; } = string.Empty;
    public string Folder { get; set; } = string.Empty;
}
