#nullable enable
using System.Text.RegularExpressions;
using FluentValidation;
using Genial.Cms.Application.Commands.Collection;

namespace Genial.Cms.Application.Validators.Collection;

public class FileDataDtoValidator : AbstractValidator<FileDataDto>
{
    public FileDataDtoValidator()
    {
        RuleFor(x => x.MaxFileSize)
            .GreaterThan(0)
            .When(x => x.MaxFileSize.HasValue)
            .WithMessage("O tamanho máximo do arquivo deve ser maior que zero")
            .WithErrorCode("040");

        // MiniTypes pode ser vazio, então não precisa de validação obrigatória
        // Apenas valida que não seja null
        RuleFor(x => x.MiniTypes)
            .NotNull()
            .WithMessage("A lista de miniTypes não pode ser nula")
            .WithErrorCode("041");
    }
}

public class InputDataDtoValidator : AbstractValidator<InputDataDto>
{
    public InputDataDtoValidator()
    {
        RuleFor(x => x.MinLength)
            .GreaterThanOrEqualTo(0)
            .When(x => x.MinLength.HasValue)
            .WithMessage("O comprimento mínimo deve ser maior ou igual a zero")
            .WithErrorCode("051");

        RuleFor(x => x.MaxLength)
            .GreaterThan(0)
            .When(x => x.MaxLength.HasValue)
            .WithMessage("O comprimento máximo deve ser maior que zero")
            .WithErrorCode("052");

        RuleFor(x => x.MaxLength)
            .GreaterThanOrEqualTo(x => x.MinLength)
            .When(x => x.MinLength.HasValue && x.MaxLength.HasValue)
            .WithMessage("O comprimento máximo deve ser maior ou igual ao comprimento mínimo")
            .WithErrorCode("053");

        RuleFor(x => x.Validation)
            .Must(BeValidRegex)
            .When(x => !string.IsNullOrWhiteSpace(x.Validation))
            .WithMessage("A expressão regular de validação é inválida")
            .WithErrorCode("054");
    }

    private bool BeValidRegex(string? regex)
    {
        if (string.IsNullOrWhiteSpace(regex))
            return true;

        try
        {
            Regex.Match("", regex);
            return true;
        }
        catch
        {
            return false;
        }
    }
}

public class TextDataDtoValidator : AbstractValidator<TextDataDto>
{
    public TextDataDtoValidator()
    {
        RuleFor(x => x.MinLength)
            .GreaterThanOrEqualTo(0)
            .When(x => x.MinLength.HasValue)
            .WithMessage("O comprimento mínimo deve ser maior ou igual a zero")
            .WithErrorCode("051");

        RuleFor(x => x.MaxLength)
            .GreaterThan(0)
            .When(x => x.MaxLength.HasValue)
            .WithMessage("O comprimento máximo deve ser maior que zero")
            .WithErrorCode("052");

        RuleFor(x => x.MaxLength)
            .GreaterThanOrEqualTo(x => x.MinLength)
            .When(x => x.MinLength.HasValue && x.MaxLength.HasValue)
            .WithMessage("O comprimento máximo deve ser maior ou igual ao comprimento mínimo")
            .WithErrorCode("053");

        RuleFor(x => x.Validation)
            .Must(BeValidRegex)
            .When(x => !string.IsNullOrWhiteSpace(x.Validation))
            .WithMessage("A expressão regular de validação é inválida")
            .WithErrorCode("054");
    }

    private bool BeValidRegex(string? regex)
    {
        if (string.IsNullOrWhiteSpace(regex))
            return true;

        try
        {
            Regex.Match("", regex);
            return true;
        }
        catch
        {
            return false;
        }
    }
}

public class NumberDataDtoValidator : AbstractValidator<NumberDataDto>
{
    public NumberDataDtoValidator()
    {
        RuleFor(x => x.Min)
            .LessThan(x => x.Max)
            .When(x => x.Min.HasValue && x.Max.HasValue)
            .WithMessage("O valor mínimo deve ser menor que o valor máximo")
            .WithErrorCode("043");
    }
}

public class EmailDataDtoValidator : AbstractValidator<EmailDataDto>
{
    public EmailDataDtoValidator()
    {
        // EmailDataDto atualmente só tem Required, sem validações adicionais
    }
}

public class SelectDataDtoValidator : AbstractValidator<SelectDataDto>
{
    public SelectDataDtoValidator()
    {
        RuleFor(x => x.Options)
            .NotEmpty()
            .WithMessage("As opções são obrigatórias para campos do tipo Select")
            .WithErrorCode("045")
            .Must(options => options != null && options.Count >= 1)
            .WithMessage("Deve haver pelo menos uma opção")
            .WithErrorCode("046");

        RuleForEach(x => x.Options)
            .SetValidator(new CollectionFieldOptionDtoValidator())
            .When(x => x.Options != null && x.Options.Count > 0);
    }
}

public class RadioDataDtoValidator : AbstractValidator<RadioDataDto>
{
    public RadioDataDtoValidator()
    {
        RuleFor(x => x.Options)
            .NotEmpty()
            .WithMessage("As opções são obrigatórias para campos do tipo Radio")
            .WithErrorCode("045")
            .Must(options => options != null && options.Count >= 1)
            .WithMessage("Deve haver pelo menos uma opção")
            .WithErrorCode("046");

        RuleForEach(x => x.Options)
            .SetValidator(new CollectionFieldOptionDtoValidator())
            .When(x => x.Options != null && x.Options.Count > 0);
    }
}

public class CheckboxDataDtoValidator : AbstractValidator<CheckboxDataDto>
{
    public CheckboxDataDtoValidator()
    {
        RuleFor(x => x.Options)
            .NotEmpty()
            .WithMessage("As opções são obrigatórias para campos do tipo Checkbox")
            .WithErrorCode("045")
            .Must(options => options != null && options.Count >= 1)
            .WithMessage("Deve haver pelo menos uma opção")
            .WithErrorCode("046");

        RuleForEach(x => x.Options)
            .SetValidator(new CollectionFieldOptionDtoValidator())
            .When(x => x.Options != null && x.Options.Count > 0);
    }
}

public class RangeDataDtoValidator : AbstractValidator<RangeDataDto>
{
    public RangeDataDtoValidator()
    {
        RuleFor(x => x.Max)
            .GreaterThan(x => x.Min)
            .WithMessage("O valor máximo deve ser maior que o valor mínimo")
            .WithErrorCode("043");
    }
}

public class CollectionFieldOptionDtoValidator : AbstractValidator<CollectionFieldOptionDto>
{
    public CollectionFieldOptionDtoValidator()
    {
        RuleFor(x => x.Label)
            .NotEmpty()
            .WithMessage("O label da opção é obrigatório")
            .WithErrorCode("047")
            .MaximumLength(200)
            .WithMessage("O label da opção não pode ter mais de 200 caracteres")
            .WithErrorCode("048");

        RuleFor(x => x.Value)
            .NotEmpty()
            .WithMessage("O valor da opção é obrigatório")
            .WithErrorCode("049")
            .MaximumLength(200)
            .WithMessage("O valor da opção não pode ter mais de 200 caracteres")
            .WithErrorCode("050");
    }
}
