#nullable enable
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Genial.Cms.Application.Commands;

namespace Genial.Cms.Application.Commands.Collection;

public class GetCollectionItemsCommand : Command<GetCollectionItemsResponse>
{
    [JsonIgnore]
    public string? CollectionId { get; set; }
    
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;

    public override bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(CollectionId) && Page > 0 && PageSize > 0;
    }
}

public class GetCollectionItemsResponse
{
    public string CollectionName { get; set; } = string.Empty;
    public string CollectionSlug { get; set; } = string.Empty;
    public List<CollectionColumnDto> Columns { get; set; } = new();
    public List<Dictionary<string, object>> Items { get; set; } = new();
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class CollectionColumnDto
{
    public string Id { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
}
