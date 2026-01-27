using System.Collections.Generic;
using Genial.Cms.Application.Queries;

namespace Genial.Cms.Application.Queries;

public class GetStatisticsByStageQuery : Query<IEnumerable<GetStatisticsByStageQueryResult>>
{
    public override bool IsValid()
    {
        return true;
    }
}

public class GetStatisticsByStageQueryResult
{
    public string StageId { get; set; } = string.Empty;
    public string StageKey { get; set; } = string.Empty;
    public string StageLabel { get; set; } = string.Empty;
    public int TotalUsers { get; set; }
    public int TotalCollections { get; set; }
    public int TotalMedia { get; set; }
    public long TotalMediaFileSize { get; set; }
    public string TotalMediaFileSizeFormatted { get; set; } = string.Empty;
}
