using FluentValidation;
using Genial.Cms.Application.Commands;

namespace Genial.Cms.Application.Validators;

public class RegisterUserCommandValidator : Validator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("O email é obrigatório")
            .WithErrorCode("001")
            .EmailAddress()
            .WithMessage("O email deve ser válido")
            .WithErrorCode("002");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("A senha é obrigatória")
            .WithErrorCode("003")
            .MinimumLength(6)
            .WithMessage("A senha deve ter no mínimo 6 caracteres")
            .WithErrorCode("004");
    }
}

