using FluentValidation;
using Genial.Cms.Application.Commands.User;

namespace Genial.Cms.Application.Validators.User;

public class UpdateUserCommandValidator : Validator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("O ID é obrigatório")
            .WithErrorCode("026");

        RuleFor(x => x.Email)
            .EmailAddress()
            .When(x => !string.IsNullOrEmpty(x.Email))
            .WithMessage("O email deve ser válido")
            .WithErrorCode("027");

        RuleFor(x => x.Password)
            .MinimumLength(6)
            .When(x => !string.IsNullOrEmpty(x.Password))
            .WithMessage("A senha deve ter no mínimo 6 caracteres")
            .WithErrorCode("028");
    }
}
