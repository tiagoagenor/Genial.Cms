using FluentValidation;
using Genial.Cms.Application.Commands.File;
using Genial.Cms.Application.Validators;

namespace Genial.Cms.Application.Validators.File;

public class UpdateMediaCommandValidator : Validator<UpdateMediaCommand>
{
    public UpdateMediaCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Id é obrigatório");
    }
}
