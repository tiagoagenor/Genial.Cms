using System;
using Genial.Cms.Application.Commands;

namespace Genial.Cms.Application.Commands.User;

public class UpdateUserCommand : Command<UpdateUserCommandResult>
{
    public string Id { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }

    public override bool IsValid()
    {
        return true;
    }
}

public class UpdateUserCommandResult
{
    public string Id { get; set; }
    public string Email { get; set; }
    public DateTime UpdatedAt { get; set; }
}
