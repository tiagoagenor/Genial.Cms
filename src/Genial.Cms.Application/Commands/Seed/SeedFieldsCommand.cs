using Genial.Cms.Application.Commands;

namespace Genial.Cms.Application.Commands.Seed;

public class SeedFieldsCommand : Command<SeedFieldsCommandResult>
{
    public override bool IsValid()
    {
        return true;
    }
}

public class SeedFieldsCommandResult
{
    public bool Success { get; set; }
    public string Message { get; set; }
}

