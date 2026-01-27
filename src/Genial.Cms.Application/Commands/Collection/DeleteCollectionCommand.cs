#nullable enable
using System.Text.Json.Serialization;
using Genial.Cms.Application.Commands;

namespace Genial.Cms.Application.Commands.Collection;

public class DeleteCollectionCommand : Command<DeleteCollectionCommandResult>
{
    [JsonIgnore]
    public string? CollectionId { get; set; }

    public override bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(CollectionId);
    }
}

public class DeleteCollectionCommandResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = default!;
}
