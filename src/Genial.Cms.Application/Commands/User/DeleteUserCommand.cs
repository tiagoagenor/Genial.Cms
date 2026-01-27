using Genial.Cms.Application.Commands;

namespace Genial.Cms.Application.Commands.User;

public class DeleteUserCommand : Command<DeleteUserCommandResult>
{
    public string Id { get; set; }

    public override bool IsValid()
    {
        return true;
    }
}

public class DeleteUserCommandResult
{
    public bool Success { get; set; }
    public string Message { get; set; }
}
