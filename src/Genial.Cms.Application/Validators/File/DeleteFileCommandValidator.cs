using FluentValidation;
using Genial.Cms.Application.Commands.File;

namespace Genial.Cms.Application.Validators.File;

public class DeleteFileCommandValidator : Validator<DeleteFileCommand>
{
    public DeleteFileCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("O id do arquivo é obrigatório")
            .WithErrorCode("073");
    }
}
