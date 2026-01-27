using System.Text.Json.Serialization;
using Genial.Cms.Application.Commands;

namespace Genial.Cms.Application.Commands.Configuration;

public class GetFilesStorageConfigurationCommand : Command<GetFilesStorageConfigurationCommandResult>
{
    public override bool IsValid()
    {
        return true;
    }
}

public class GetFilesStorageConfigurationCommandResult
{
    public string Key { get; set; }
    public bool Status { get; set; }
    public GetFilesStorageValuesDto Values { get; set; }
}

public class GetFilesStorageValuesDto
{
    public string Bucket { get; set; }
    public string Region { get; set; }
    
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string Endpoint { get; set; }
    
    public string AccessKeyId { get; set; }
    public string SecretAccessKey { get; set; }
    public string Folder { get; set; }
}

public class FilesStorageValuesDto
{
    public string Bucket { get; set; }
    public string Region { get; set; }
    public string Endpoint { get; set; }
    public string AccessKeyId { get; set; }
    public string SecretAccessKey { get; set; }
    public string Folder { get; set; }
}
