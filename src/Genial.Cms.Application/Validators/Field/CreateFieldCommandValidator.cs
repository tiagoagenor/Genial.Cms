using System.Linq;
using FluentValidation;
using Genial.Cms.Application.Commands.Field;

namespace Genial.Cms.Application.Validators.Field;

public class CreateFieldCommandValidator : Validator<CreateFieldCommand>
{
    public CreateFieldCommandValidator()
    {
        RuleFor(x => x.Key)
            .NotEmpty()
            .WithMessage("A chave do field é obrigatória")
            .WithErrorCode("010")
            .MaximumLength(100)
            .WithMessage("A chave do field não pode ter mais de 100 caracteres")
            .WithErrorCode("011");

        RuleFor(x => x.Label)
            .NotEmpty()
            .WithMessage("O label do field é obrigatório")
            .WithErrorCode("012")
            .MaximumLength(200)
            .WithMessage("O label do field não pode ter mais de 200 caracteres")
            .WithErrorCode("013");

        RuleFor(x => x.Type)
            .NotEmpty()
            .WithMessage("O tipo do field é obrigatório")
            .WithErrorCode("014")
            .Must(type => new[] { "text", "number", "email", "date", "boolean", "select" }.Contains(type.ToLower()))
            .WithMessage("O tipo do field deve ser: text, number, email, date, boolean ou select")
            .WithErrorCode("015");
    }
}

