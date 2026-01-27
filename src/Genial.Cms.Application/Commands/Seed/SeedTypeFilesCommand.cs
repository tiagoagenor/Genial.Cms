using Genial.Cms.Application.Commands;

namespace Genial.Cms.Application.Commands.Seed;

public class SeedTypeFilesCommand : Command<SeedTypeFilesCommandResult>
{
    public override bool IsValid()
    {
        return true;
    }
}

public class SeedTypeFilesCommandResult
{
    public bool Success { get; set; }
    public string Message { get; set; }
}
