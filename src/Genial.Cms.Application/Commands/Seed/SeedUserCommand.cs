using Genial.Cms.Application.Commands;

namespace Genial.Cms.Application.Commands.Seed;

public class SeedUserCommand : Command<SeedUserCommandResult>
{
    public override bool IsValid()
    {
        return true;
    }
}

public class SeedUserCommandResult
{
    public bool Success { get; set; }
    public string Message { get; set; }
}
