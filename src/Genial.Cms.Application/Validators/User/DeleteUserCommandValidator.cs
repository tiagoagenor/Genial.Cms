using FluentValidation;
using Genial.Cms.Application.Commands.User;

namespace Genial.Cms.Application.Validators.User;

public class DeleteUserCommandValidator : Validator<DeleteUserCommand>
{
    public DeleteUserCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("O ID é obrigatório")
            .WithErrorCode("030");
    }
}
