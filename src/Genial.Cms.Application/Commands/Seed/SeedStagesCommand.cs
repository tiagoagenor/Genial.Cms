using Genial.Cms.Application.Commands;

namespace Genial.Cms.Application.Commands.Seed;

public class SeedStagesCommand : Command<SeedStagesCommandResult>
{
    public override bool IsValid()
    {
        return true;
    }
}

public class SeedStagesCommandResult
{
    public bool Success { get; set; }
    public string Message { get; set; }
}

