#nullable enable
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Genial.Cms.Application.Commands;
using Genial.Cms.Domain.Dtos;

namespace Genial.Cms.Application.Commands.Collection;

public class CreateCollectionItemCommand : Command<CreateCollectionItemResponse>
{
    [JsonIgnore]
    public string? CollectionId { get; set; }
    
    [JsonIgnore]
    public JwtUserData? UserData { get; set; }
    
    public Dictionary<string, object> Data { get; set; } = new();

    public override bool IsValid()
    {
        // CollectionId é preenchido no controller a partir da URL, então não validamos aqui
        // A validação do CollectionId será feita no handler
        return Data != null;
    }
}

public class CreateCollectionItemResponse
{
    public string Id { get; set; } = default!;
    public Dictionary<string, object> Data { get; set; } = new();
}
