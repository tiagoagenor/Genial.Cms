using FluentValidation;
using Genial.Cms.Application.Commands.User;

namespace Genial.Cms.Application.Validators.User;

public class CreateUserCommandValidator : Validator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("O email é obrigatório")
            .WithErrorCode("022")
            .EmailAddress()
            .WithMessage("O email deve ser válido")
            .WithErrorCode("023");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("A senha é obrigatória")
            .WithErrorCode("024")
            .MinimumLength(6)
            .WithMessage("A senha deve ter no mínimo 6 caracteres")
            .WithErrorCode("025");
    }
}
