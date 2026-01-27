#nullable enable
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Genial.Cms.Application.Commands;
using Genial.Cms.Domain.Dtos;

namespace Genial.Cms.Application.Commands.Collection;

public class UpdateCollectionItemCommand : Command<UpdateCollectionItemResponse>
{
    [JsonIgnore]
    public string? CollectionId { get; set; }
    
    [JsonIgnore]
    public string? ItemId { get; set; }
    
    [JsonIgnore]
    public JwtUserData? UserData { get; set; }
    
    public Dictionary<string, object> Data { get; set; } = new();

    public override bool IsValid()
    {
        return Data != null;
    }
}

public class UpdateCollectionItemResponse
{
    public string Id { get; set; } = default!;
    public Dictionary<string, object> Data { get; set; } = new();
}
