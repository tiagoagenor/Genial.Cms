using System.Collections.Generic;
using Genial.Cms.Application.Commands;

namespace Genial.Cms.Application.Commands.Collection;

public class GetCollectionsCommand : Command<GetCollectionsCommandResult>
{
    public string StageId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;

    public override bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(StageId) && Page > 0 && PageSize > 0;
    }
}

public class GetCollectionsCommandResult
{
    public List<CollectionNameDto> Data { get; set; } = new();
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class CollectionNameDto
{
    public string Id { get; set; }
    public string Name { get; set; }
}
