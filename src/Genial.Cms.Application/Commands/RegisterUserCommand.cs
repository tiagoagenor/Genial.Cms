using Genial.Cms.Application.Commands;

namespace Genial.Cms.Application.Commands;

public class RegisterUserCommand : Command<RegisterUserCommandResult>
{
    public string Email { get; set; }
    public string Password { get; set; }

    public override bool IsValid()
    {
        return true;
    }
}

public class RegisterUserCommandResult
{
    public string Id { get; set; }
    public string Email { get; set; }
}

