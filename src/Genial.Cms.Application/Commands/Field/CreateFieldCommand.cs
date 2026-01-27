using Genial.Cms.Application.Commands;

namespace Genial.Cms.Application.Commands.Field;

public class CreateFieldCommand : Command<CreateFieldCommandResult>
{
    public string Key { get; set; }
    public string Label { get; set; }
    public string Type { get; set; }
    public bool Active { get; set; }
    public int Order { get; set; }

    public override bool IsValid()
    {
        return true;
    }
}

public class CreateFieldCommandResult
{
    public string Id { get; set; }
    public string Key { get; set; }
    public string Label { get; set; }
    public string Type { get; set; }
    public bool Active { get; set; }
    public int Order { get; set; }
}

