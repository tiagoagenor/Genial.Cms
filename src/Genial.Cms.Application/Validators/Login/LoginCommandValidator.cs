using FluentValidation;
using Genial.Cms.Application.Commands.Login;

namespace Genial.Cms.Application.Validators.Login;

public class LoginCommandValidator : Validator<LoginCommand>
{
    public LoginCommandValidator()
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
            .WithErrorCode("003");
    }
}

