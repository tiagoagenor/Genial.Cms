#nullable enable
using Genial.Cms.Application.Commands;
using Genial.Cms.Domain.Dtos;

namespace Genial.Cms.Application.Commands.Stage;

public class ChangeStageCommand : Command<ChangeStageCommandResult>
{
    public string StageId { get; set; }
    public JwtUserData? UserData { get; set; }

    public override bool IsValid()
    {
        return true;
    }
}

public class ChangeStageCommandResult
{
    public string Token { get; set; }
}

