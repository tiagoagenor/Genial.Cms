using Genial.Cms.Application.Commands;

namespace Genial.Cms.Application.Commands.Login;

public class LoginCommand : Command<LoginCommandResult>
{
    public string Email { get; set; }
    public string Password { get; set; }

    public override bool IsValid()
    {
        return true;
    }
}

public class LoginCommandResult
{
    public string Token { get; set; }
}

public class RefreshTokenCommandResult
{
    public string Token { get; set; }
}

