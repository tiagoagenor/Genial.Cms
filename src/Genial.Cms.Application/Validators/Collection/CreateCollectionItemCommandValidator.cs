#nullable enable
using FluentValidation;
using Genial.Cms.Application.Commands.Collection;

namespace Genial.Cms.Application.Validators.Collection;

public class CreateCollectionItemCommandValidator : AbstractValidator<CreateCollectionItemCommand>
{
    public CreateCollectionItemCommandValidator()
    {
        // CollectionId é preenchido no controller a partir da URL, então não validamos aqui
        // A validação do CollectionId será feita no handler
        
        RuleFor(x => x.Data)
            .NotNull()
            .WithMessage("Os dados são obrigatórios")
            .WithErrorCode("064")
            .Must(data => data != null && data.Count > 0)
            .WithMessage("Deve haver pelo menos um campo nos dados")
            .WithErrorCode("065");
    }
}
