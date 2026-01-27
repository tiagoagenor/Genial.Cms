using FluentValidation;
using Genial.Cms.Application.Commands.File;

namespace Genial.Cms.Application.Validators.File;

public class GetMediaByIdCommandValidator : Validator<GetMediaByIdCommand>
{
    public GetMediaByIdCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("O id do arquivo é obrigatório")
            .WithErrorCode("076");
    }
}

