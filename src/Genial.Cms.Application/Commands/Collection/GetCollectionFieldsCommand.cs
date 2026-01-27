using System.Collections.Generic;
using Genial.Cms.Application.Commands;

namespace Genial.Cms.Application.Commands.Collection;

public class GetCollectionFieldsCommand : Command<GetCollectionFieldsCommandResult>
{
    public string CollectionId { get; set; }

    public override bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(CollectionId);
    }
}

public class GetCollectionFieldsCommandResult
{
    public string CollectionId { get; set; }
    public string CollectionName { get; set; }
    public List<CollectionFieldDto> Fields { get; set; } = new();
}

public class CollectionFieldDto
{
    public string Type { get; set; }
    public string Name { get; set; }
    public string Slug { get; set; }
    public object Data { get; set; }
    public System.DateTime CreatedAt { get; set; }
    public System.DateTime UpdatedAt { get; set; }
}
