using FluentValidation;
using Genial.Cms.Application.Commands.File;

namespace Genial.Cms.Application.Validators.File;

public class UploadFileCommandValidator : Validator<UploadFileCommand>
{
    public UploadFileCommandValidator()
    {
        RuleFor(x => x.File)
            .NotNull()
            .WithMessage("O arquivo é obrigatório")
            .WithErrorCode("070")
            .Must(file => file != null && file.Length > 0)
            .WithMessage("O arquivo não pode estar vazio")
            .WithErrorCode("070");

        RuleFor(x => x.File)
            .Must(file => file == null || file.Length <= 2L * 1024 * 1024 * 1024) // 2GB
            .When(x => x.File != null)
            .WithMessage("O tamanho do arquivo não pode exceder 2GB")
            .WithErrorCode("072");
    }
}
