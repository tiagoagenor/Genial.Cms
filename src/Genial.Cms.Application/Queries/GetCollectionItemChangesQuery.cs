using System.Collections.Generic;
using Genial.Cms.Application.Queries;

namespace Genial.Cms.Application.Queries;

public class GetCollectionItemChangesQuery : Query<IEnumerable<GetCollectionItemChangesQueryResult>>
{
    public string CollectionId { get; set; } = string.Empty;
    public string ItemId { get; set; } = string.Empty;

    public override bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(CollectionId) && !string.IsNullOrWhiteSpace(ItemId);
    }
}

public class GetCollectionItemChangesQueryResult
{
    public string Id { get; set; } = string.Empty;
    public string CollectionId { get; set; } = string.Empty;
    public string ItemId { get; set; } = string.Empty;
    public UserInfo User { get; set; } = new();
    public int ChangeType { get; set; }
    public object? BeforeData { get; set; }
    public object? AfterData { get; set; }
    public System.DateTime CreatedAt { get; set; }
}

public class UserInfo
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}
