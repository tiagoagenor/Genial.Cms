using System;
using System.Collections.Generic;
using Genial.Cms.Application.Queries;

namespace Genial.Cms.Application.Queries;

public class GetUsersQuery : Query<IEnumerable<GetUsersQueryResult>>
{
    public override bool IsValid()
    {
        return true;
    }
}

public class GetUsersQueryResult
{
    public string Id { get; set; }
    public string Email { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
