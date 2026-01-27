using FluentValidation;
using Genial.Cms.Application.Commands.Collection;

namespace Genial.Cms.Application.Validators.Collection;

public class DeleteCollectionCommandValidator : Validator<DeleteCollectionCommand>
{
    public DeleteCollectionCommandValidator()
    {
        // CollectionId é validado no handler, não precisa validar aqui
        // pois é preenchido pelo controller a partir da URL
    }
}
