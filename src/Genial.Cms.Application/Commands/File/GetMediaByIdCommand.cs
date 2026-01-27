#nullable enable
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Genial.Cms.Application.Commands;

namespace Genial.Cms.Application.Commands.File;

public class GetMediaByIdCommand : Command<GetMediaByIdCommandResult>
{
    public string Id { get; set; } = default!;

    public override bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(Id);
    }
}

public class GetMediaByIdCommandResult
{
    public MediaByIdDto Data { get; set; } = default!;
}

public class MediaByIdDto
{
    public string Id { get; set; } = default!;

    [JsonPropertyName("originalFileName")]
    public string OriginalFileName { get; set; } = default!;

    [JsonPropertyName("fileNameUrl")]
    public string FileNameUrl { get; set; } = default!;

    public string ContentType { get; set; } = default!;
    public long FileSize { get; set; }
    public string Url { get; set; } = default!;
    public List<string> Tags { get; set; } = new();
    public string Extension { get; set; } = default!;
    public string StageId { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

