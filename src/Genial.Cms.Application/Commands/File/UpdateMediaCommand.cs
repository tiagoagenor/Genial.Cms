#nullable enable
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Genial.Cms.Application.Commands;
using Genial.Cms.Domain.Dtos;

namespace Genial.Cms.Application.Commands.File;

public class UpdateMediaCommand : Command<UpdateMediaCommandResult>
{
    public string Id { get; set; } = default!;
    public List<string>? Tags { get; set; }
    
    [JsonIgnore]
    public JwtUserData? UserData { get; set; }

    public override bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(Id);
    }
}

public class UpdateMediaCommandResult
{
    public string Id { get; set; } = default!;
    public List<string> Tags { get; set; } = new();
    public string StageId { get; set; } = default!;
    public System.DateTime UpdatedAt { get; set; }
}
