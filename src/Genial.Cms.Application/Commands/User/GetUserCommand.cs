using System;
using Genial.Cms.Application.Commands;

namespace Genial.Cms.Application.Commands.User;

public class GetUserCommand : Command<GetUserCommandResult>
{
    public string Id { get; set; }

    public override bool IsValid()
    {
        return true;
    }
}

public class GetUserCommandResult
{
    public string Id { get; set; }
    public string Email { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
