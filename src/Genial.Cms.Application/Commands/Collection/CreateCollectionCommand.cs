#nullable enable
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Genial.Cms.Application.Commands;
using Genial.Cms.Domain.Dtos;

namespace Genial.Cms.Application.Commands.Collection;

public class CreateCollectionCommand : Command<CreateCollectionCommandResult>
{
    public string Name { get; set; } = default!;
    public List<CollectionFieldItemDto> Fields { get; set; } = new();
    
    [JsonIgnore]
    public JwtUserData? UserData { get; set; }

    public override bool IsValid()
    {
        return true;
    }
}

public class CreateCollectionCommandResult
{
    public string Id { get; set; }
    public List<CollectionFieldItemResultDto> Fields { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public sealed class CollectionFieldItemResultDto
{
    public string Id { get; set; } = default!;
    public string Type { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string Slug { get; set; } = default!;
    public object Data { get; set; } = default!;
}
