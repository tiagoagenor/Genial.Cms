using System.Collections.Generic;
using Genial.Cms.Application.Queries;

namespace Genial.Cms.Application.Queries;

public class GetCollectionChangesQuery : Query<GetCollectionChangesQueryResult>
{
    public string CollectionId { get; set; } = string.Empty;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;

    public override bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(CollectionId) && Page > 0 && PageSize > 0;
    }
}

public class GetCollectionChangesQueryResult
{
    public List<GetCollectionItemChangesQueryResult> Data { get; set; } = new();
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}
