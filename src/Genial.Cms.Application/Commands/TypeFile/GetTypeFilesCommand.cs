using System.Collections.Generic;
using Genial.Cms.Application.Commands;

namespace Genial.Cms.Application.Commands.TypeFile;

public class GetTypeFilesCommand : Command<GetTypeFilesResponse>
{
    public override bool IsValid()
    {
        return true; // Sem validação necessária, apenas retorna todos os TypeFiles
    }
}

public class GetTypeFilesResponse
{
    public List<TypeFileDto> Data { get; set; } = new();
}

public class TypeFileDto
{
    public string Id { get; set; }
    public string Key { get; set; }
    public string Value { get; set; }
    public int Order { get; set; }
    public string Category { get; set; }
    public System.DateTime CreatedAt { get; set; }
    public System.DateTime UpdatedAt { get; set; }
}
