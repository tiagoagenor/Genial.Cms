using Genial.Cms.Domain.SeedWork;

namespace Genial.Cms.Domain.Aggregates;

public class CollectionItemChangeTypeEnum : Enumeration
{
    public static readonly CollectionItemChangeTypeEnum Add = new(1, "Add");
    public static readonly CollectionItemChangeTypeEnum Edit = new(2, "Edit");
    public static readonly CollectionItemChangeTypeEnum Delete = new(3, "Delete");

    private CollectionItemChangeTypeEnum(int id, string name) : base(id, name)
    {
    }
}
