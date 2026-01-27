#nullable enable
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Genial.Cms.Application.Commands;
using Genial.Cms.Domain.Dtos;

namespace Genial.Cms.Application.Commands.Collection;

public class UpdateCollectionCommand : Command<UpdateCollectionCommandResult>
{
    [JsonIgnore]
    public string? CollectionId { get; set; }
    
    public string Name { get; set; } = default!;
    public List<CollectionFieldItemDto> Fields { get; set; } = new();
    
    [JsonIgnore]
    public JwtUserData? UserData { get; set; }

    public override bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(CollectionId) && !string.IsNullOrWhiteSpace(Name);
    }
}

public class UpdateCollectionCommandResult
{
    public string Id { get; set; } = default!;
    public List<CollectionFieldItemResultDto> Fields { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
