#nullable enable
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Genial.Cms.Application.Commands;

namespace Genial.Cms.Application.Commands.Collection;

public class GetCollectionItemsBySlugCommand : Command<GetCollectionItemsBySlugResponse>
{
    [JsonIgnore]
    public string? StageKey { get; set; }
    
    [JsonIgnore]
    public string? Slug { get; set; }
    
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;

    public override bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(StageKey) && !string.IsNullOrWhiteSpace(Slug) && Page > 0 && PageSize > 0;
    }
}

public class GetCollectionItemsBySlugResponse
{
    public List<Dictionary<string, object>> Items { get; set; } = new();
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}
