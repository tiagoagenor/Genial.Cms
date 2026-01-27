using System.Collections.Generic;
using Genial.Cms.Application.Queries;
using Genial.Cms.Domain.Aggregates;

namespace Genial.Cms.Application.Queries;

public class GetStagesQuery : Query<IEnumerable<Stage>>
{
    public override bool IsValid()
    {
        return true;
    }
}

