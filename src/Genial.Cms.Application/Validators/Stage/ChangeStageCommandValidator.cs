using FluentValidation;
using Genial.Cms.Application.Commands.Stage;

namespace Genial.Cms.Application.Validators.Stage;

public class ChangeStageCommandValidator : Validator<ChangeStageCommand>
{
    public ChangeStageCommandValidator()
    {
        RuleFor(x => x.StageId)
            .NotEmpty()
            .WithMessage("O ID do stage é obrigatório")
            .WithErrorCode("001");
    }
}

