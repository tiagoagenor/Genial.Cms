using Genial.Cms.Application.Commands;

namespace Genial.Cms.Application.Commands.Configuration;

public class SaveFilesStorageConfigurationCommand : Command<SaveFilesStorageConfigurationCommandResult>
{
    public bool Status { get; set; }
    public FilesStorageValuesDto Values { get; set; }

    public override bool IsValid()
    {
        return Values != null && !string.IsNullOrWhiteSpace(Values.Bucket) && !string.IsNullOrWhiteSpace(Values.Region);
    }
}

public class SaveFilesStorageConfigurationCommandResult
{
    public string Id { get; set; }
    public string Key { get; set; }
    public bool Status { get; set; }
    public FilesStorageValuesDto Values { get; set; }
    public System.DateTime CreatedAt { get; set; }
    public System.DateTime UpdatedAt { get; set; }
}
