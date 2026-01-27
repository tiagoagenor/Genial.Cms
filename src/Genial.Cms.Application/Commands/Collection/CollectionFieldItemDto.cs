#nullable enable
namespace Genial.Cms.Application.Commands.Collection;

public sealed class CollectionFieldItemDto
{
    public string Type { get; set; } = default!;
    public string Name { get; set; } = default!;

    // Data específica para cada tipo
    public FileDataDto? FileData { get; set; }
    public InputDataDto? InputData { get; set; }
    public TextDataDto? TextData { get; set; }
    public NumberDataDto? NumberData { get; set; }
    public EmailDataDto? EmailData { get; set; }
    public SelectDataDto? SelectData { get; set; }
    public RadioDataDto? RadioData { get; set; }
    public BoolDataDto? BoolData { get; set; }
    public CheckboxDataDto? CheckboxData { get; set; }
    public RangeDataDto? RangeData { get; set; }
    public ColorDataDto? ColorData { get; set; }
}
