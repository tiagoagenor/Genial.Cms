using Genial.Cms.Application.Commands;

namespace Genial.Cms.Application.Commands.Seed;

public class SeedCommand : Command<SeedCommandResult>
{
    public override bool IsValid()
    {
        return true;
    }
}

public class SeedCommandResult
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public string UserId { get; set; }
    public string Email { get; set; }
}

