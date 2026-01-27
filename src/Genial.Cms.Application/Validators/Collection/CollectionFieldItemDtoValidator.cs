using System;
using System.Linq;
using FluentValidation;
using Genial.Cms.Application.Commands.Collection;
using Genial.Cms.Domain.Aggregates;

namespace Genial.Cms.Application.Validators.Collection;

public class CollectionFieldItemDtoValidator : AbstractValidator<CollectionFieldItemDto>
{
    public CollectionFieldItemDtoValidator()
    {
        RuleFor(x => x.Type)
            .NotEmpty()
            .WithMessage("O tipo do campo é obrigatório")
            .WithErrorCode("038")
            .Must(type =>
            {
                if (string.IsNullOrWhiteSpace(type))
                    return false;

                var validTypes = FieldTypeEnum.GetAll<FieldTypeEnum>();
                return validTypes.Any(t => t.Name.Equals(type, StringComparison.OrdinalIgnoreCase));
            })
            .WithMessage(type =>
            {
                var validTypes = FieldTypeEnum.GetAll<FieldTypeEnum>();
                var validTypeNames = string.Join(", ", validTypes.Select(t => t.Name));
                return $"O tipo do campo é inválido. Tipos válidos: {validTypeNames}";
            })
            .WithErrorCode("039");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("O nome do campo é obrigatório")
            .WithErrorCode("036")
            .MaximumLength(200)
            .WithMessage("O nome do campo não pode ter mais de 200 caracteres")
            .WithErrorCode("037");

        // Validações específicas por tipo
        When(x => !string.IsNullOrEmpty(x.Type) && x.Type.Equals(FieldTypeEnum.File.Name, StringComparison.OrdinalIgnoreCase), () =>
        {
            RuleFor(x => x.FileData)
                .NotNull()
                .WithMessage("Os dados do tipo File são obrigatórios")
                .WithErrorCode("040");

            RuleFor(x => x.FileData)
                .SetValidator(new FileDataDtoValidator())
                .When(x => x.FileData != null);
        });

        When(x => !string.IsNullOrEmpty(x.Type) && x.Type.Equals(FieldTypeEnum.Input.Name, StringComparison.OrdinalIgnoreCase), () =>
        {
            RuleFor(x => x.InputData)
                .NotNull()
                .WithMessage("Os dados do tipo Input são obrigatórios")
                .WithErrorCode("040");

            RuleFor(x => x.InputData)
                .SetValidator(new InputDataDtoValidator())
                .When(x => x.InputData != null);
        });

        When(x => !string.IsNullOrEmpty(x.Type) && x.Type.Equals(FieldTypeEnum.Text.Name, StringComparison.OrdinalIgnoreCase), () =>
        {
            RuleFor(x => x.TextData)
                .NotNull()
                .WithMessage("Os dados do tipo Text são obrigatórios")
                .WithErrorCode("040");

            RuleFor(x => x.TextData)
                .SetValidator(new TextDataDtoValidator())
                .When(x => x.TextData != null);
        });

        When(x => !string.IsNullOrEmpty(x.Type) && x.Type.Equals(FieldTypeEnum.Number.Name, StringComparison.OrdinalIgnoreCase), () =>
        {
            RuleFor(x => x.NumberData)
                .NotNull()
                .WithMessage("Os dados do tipo Number são obrigatórios")
                .WithErrorCode("040");

            RuleFor(x => x.NumberData)
                .SetValidator(new NumberDataDtoValidator())
                .When(x => x.NumberData != null);
        });

        When(x => !string.IsNullOrEmpty(x.Type) && x.Type.Equals(FieldTypeEnum.Email.Name, StringComparison.OrdinalIgnoreCase), () =>
        {
            RuleFor(x => x.EmailData)
                .NotNull()
                .WithMessage("Os dados do tipo Email são obrigatórios")
                .WithErrorCode("040");

            RuleFor(x => x.EmailData)
                .SetValidator(new EmailDataDtoValidator())
                .When(x => x.EmailData != null);
        });

        When(x => !string.IsNullOrEmpty(x.Type) && x.Type.Equals(FieldTypeEnum.Select.Name, StringComparison.OrdinalIgnoreCase), () =>
        {
            RuleFor(x => x.SelectData)
                .NotNull()
                .WithMessage("Os dados do tipo Select são obrigatórios")
                .WithErrorCode("040");

            RuleFor(x => x.SelectData)
                .SetValidator(new SelectDataDtoValidator())
                .When(x => x.SelectData != null);
        });

        When(x => !string.IsNullOrEmpty(x.Type) && x.Type.Equals(FieldTypeEnum.Radio.Name, StringComparison.OrdinalIgnoreCase), () =>
        {
            RuleFor(x => x.RadioData)
                .NotNull()
                .WithMessage("Os dados do tipo Radio são obrigatórios")
                .WithErrorCode("040");

            RuleFor(x => x.RadioData)
                .SetValidator(new RadioDataDtoValidator())
                .When(x => x.RadioData != null);
        });

        When(x => !string.IsNullOrEmpty(x.Type) && x.Type.Equals(FieldTypeEnum.Checkbox.Name, StringComparison.OrdinalIgnoreCase), () =>
        {
            RuleFor(x => x.CheckboxData)
                .NotNull()
                .WithMessage("Os dados do tipo Checkbox são obrigatórios")
                .WithErrorCode("040");

            RuleFor(x => x.CheckboxData)
                .SetValidator(new CheckboxDataDtoValidator())
                .When(x => x.CheckboxData != null);
        });

        When(x => !string.IsNullOrEmpty(x.Type) && x.Type.Equals(FieldTypeEnum.Range.Name, StringComparison.OrdinalIgnoreCase), () =>
        {
            RuleFor(x => x.RangeData)
                .NotNull()
                .WithMessage("Os dados do tipo Range são obrigatórios")
                .WithErrorCode("040");

            RuleFor(x => x.RangeData)
                .SetValidator(new RangeDataDtoValidator())
                .When(x => x.RangeData != null);
        });
    }
}
