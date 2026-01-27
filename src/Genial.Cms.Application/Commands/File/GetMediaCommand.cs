using System.Collections.Generic;
using System.Text.Json.Serialization;
using Genial.Cms.Application.Commands;

namespace Genial.Cms.Application.Commands.File;

public class GetMediaCommand : Command<GetMediaCommandResult>
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public List<string>? Tags { get; set; }
    public string? ContentType { get; set; }
    public string? Extension { get; set; }
    public string? StageId { get; set; } // Filtrar por stage
    public string SortBy { get; set; } = "createdAt"; // createdAt, name, fileSize
    public string SortDirection { get; set; } = "desc"; // asc, desc

    public override bool IsValid()
    {
        return Page > 0 && PageSize > 0;
    }
}

public class GetMediaCommandResult
{
    public List<MediaDto> Data { get; set; } = new();
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class MediaDto
{
    public string Id { get; set; } = default!;
    
    [JsonPropertyName("originalFileName")]
    public string FileName { get; set; } = default!;
    
    [JsonPropertyName("fileNameUrl")]
    public string FileNameUrl { get; set; } = default!;
    
    public string ContentType { get; set; } = default!;
    public long FileSize { get; set; }
    public string Url { get; set; } = default!;
    public List<string> Tags { get; set; } = new();
    public string Extension { get; set; } = default!;
    public string StageId { get; set; } = default!;
    public System.DateTime CreatedAt { get; set; }
    public System.DateTime UpdatedAt { get; set; }
}
