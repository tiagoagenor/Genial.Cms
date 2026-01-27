#nullable enable
using System.Text.Json.Serialization;
using Genial.Cms.Application.Commands;
using Genial.Cms.Domain.Dtos;

namespace Genial.Cms.Application.Commands.File;

public class DeleteFileCommand : Command<DeleteFileCommandResult>
{
    public string Id { get; set; } = default!; // MongoDB ObjectId (ex: 65f0c1c2a1b2c3d4e5f67890)
    
    [JsonIgnore]
    public JwtUserData? UserData { get; set; }

    public override bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(Id);
    }
}

public class DeleteFileCommandResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = default!;
    public string? Id { get; set; }
    public string? FileNameUrl { get; set; }
}
