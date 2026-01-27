using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Genial.Cms.Domain.Aggregates;

public class Collection
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    [BsonElement("name")]
    public string Name { get; set; }

    [BsonElement("slug")]
    public string Slug { get; set; }

    [BsonElement("stageId")]
    public string StageId { get; set; }

    [BsonElement("collection")]
    public string CollectionName { get; set; }

    [BsonElement("fields")]
    public List<CollectionField> Fields { get; set; } = new();

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; }

    [BsonElement("updatedAt")]
    public DateTime UpdatedAt { get; set; }

    public static string GenerateSlug(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return string.Empty;

        // Converter para minúsculas
        var slug = name.ToLowerInvariant();

        // Remover caracteres especiais e acentos (manter apenas letras, números, espaços e hífens)
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[^a-z0-9\s-]", "");

        // Substituir espaços por underscore
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"\s+", "_");

        // Substituir hífens por underscore também
        slug = slug.Replace("-", "_");

        // Remover underscores múltiplos
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"_+", "_");

        // Remover underscores no início e fim
        slug = slug.Trim('_');

        return slug;
    }
}
