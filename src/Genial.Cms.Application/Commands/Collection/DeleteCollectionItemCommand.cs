#nullable enable
using System.Text.Json.Serialization;
using Genial.Cms.Application.Commands;
using Genial.Cms.Domain.Dtos;

namespace Genial.Cms.Application.Commands.Collection;

public class DeleteCollectionItemCommand : Command<DeleteCollectionItemResponse>
{
    [JsonIgnore]
    public string? CollectionId { get; set; }
    
    [JsonIgnore]
    public string? ItemId { get; set; }
    
    [JsonIgnore]
    public JwtUserData? UserData { get; set; }

    public override bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(CollectionId) && !string.IsNullOrWhiteSpace(ItemId);
    }
}

public class DeleteCollectionItemResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = default!;
}
