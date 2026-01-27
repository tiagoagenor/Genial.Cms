using FluentValidation;
using Genial.Cms.Application.Commands.User;

namespace Genial.Cms.Application.Validators.User;

public class GetUserCommandValidator : Validator<GetUserCommand>
{
    public GetUserCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("O ID é obrigatório")
            .WithErrorCode("029");
    }
}
