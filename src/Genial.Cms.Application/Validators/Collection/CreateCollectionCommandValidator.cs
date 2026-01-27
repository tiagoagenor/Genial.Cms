using FluentValidation;
using Genial.Cms.Application.Commands.Collection;

namespace Genial.Cms.Application.Validators.Collection;

public class CreateCollectionCommandValidator : Validator<CreateCollectionCommand>
{
    public CreateCollectionCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("O campo 'name' é obrigatório")
            .WithErrorCode("063");

        RuleFor(x => x.Fields)
            .NotEmpty()
            .WithMessage("A lista de campos é obrigatória")
            .WithErrorCode("055")
            .Must(fields => fields != null && fields.Count > 0)
            .WithMessage("Deve haver pelo menos um campo")
            .WithErrorCode("056");

        RuleForEach(x => x.Fields)
            .SetValidator(new CollectionFieldItemDtoValidator())
            .When(x => x.Fields != null && x.Fields.Count > 0);
    }
}

