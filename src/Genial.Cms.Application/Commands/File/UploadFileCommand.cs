#nullable enable
using System;
using System.Text.Json.Serialization;
using Genial.Cms.Application.Commands;
using Genial.Cms.Domain.Dtos;
using Microsoft.AspNetCore.Http;

namespace Genial.Cms.Application.Commands.File;

public class UploadFileCommand : Command<UploadFileCommandResult>
{
    public IFormFile File { get; set; } = default!;
    
    [JsonIgnore]
    public JwtUserData? UserData { get; set; }

    public override bool IsValid()
    {
        return File != null && File.Length > 0;
    }
}

public class UploadFileCommandResult
{
    public string FileId { get; set; } = default!;
    public string FileName { get; set; } = default!;
    public string FilePath { get; set; } = default!;
    public long FileSize { get; set; }
    public string ContentType { get; set; } = default!;
    public string Url { get; set; } = default!;
    public DateTime UploadedAt { get; set; }
}
