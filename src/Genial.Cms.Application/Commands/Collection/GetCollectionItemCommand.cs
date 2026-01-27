#nullable enable
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Genial.Cms.Application.Commands;

namespace Genial.Cms.Application.Commands.Collection;

public class GetCollectionItemCommand : Command<GetCollectionItemResponse>
{
    [JsonIgnore]
    public string? CollectionId { get; set; }

    [JsonIgnore]
    public string? ItemId { get; set; }

    public override bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(CollectionId) && !string.IsNullOrWhiteSpace(ItemId);
    }
}

public class GetCollectionItemResponse
{
    public string CollectionName { get; set; } = string.Empty;
    public List<CollectionColumnDto> Columns { get; set; } = new();
    public Dictionary<string, object> Item { get; set; } = new();
}
