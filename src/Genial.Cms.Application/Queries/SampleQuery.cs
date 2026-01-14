using System.Collections.Generic;

namespace Genial.Cms.Application.Queries;

public class SampleQuery : Query<IEnumerable<string>>
{
    public bool ForceClientException { get; }

    public SampleQuery(bool forceClientException)
    {
        ForceClientException = forceClientException;
    }

    public override bool IsValid()
    {
        return true;
    }
}
