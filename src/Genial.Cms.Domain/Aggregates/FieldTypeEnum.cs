using Genial.Cms.Domain.SeedWork;

namespace Genial.Cms.Domain.Aggregates;

public class FieldTypeEnum : Enumeration
{
    public static readonly FieldTypeEnum Input = new(1, "input");
    public static readonly FieldTypeEnum Text = new(2, "text");
    public static readonly FieldTypeEnum Number = new(3, "number");
    public static readonly FieldTypeEnum Email = new(4, "email");
    public static readonly FieldTypeEnum Select = new(5, "select");
    public static readonly FieldTypeEnum Radio = new(6, "radio");
    public static readonly FieldTypeEnum Bool = new(7, "bool");
    public static readonly FieldTypeEnum Checkbox = new(8, "checkbox");
    public static readonly FieldTypeEnum File = new(9, "file");
    public static readonly FieldTypeEnum Range = new(10, "range");
    public static readonly FieldTypeEnum Color = new(11, "color");

    private FieldTypeEnum(int id, string name) : base(id, name)
    {
    }
}
