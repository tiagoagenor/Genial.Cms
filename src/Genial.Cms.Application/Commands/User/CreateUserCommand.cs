using System;
using Genial.Cms.Application.Commands;

namespace Genial.Cms.Application.Commands.User;

public class CreateUserCommand : Command<CreateUserCommandResult>
{
    public string Email { get; set; }
    public string Password { get; set; }

    public override bool IsValid()
    {
        return true;
    }
}

public class CreateUserCommandResult
{
    public string Id { get; set; }
    public string Email { get; set; }
    public DateTime CreatedAt { get; set; }
}
