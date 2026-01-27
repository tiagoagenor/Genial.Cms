using System;
using System.Text.RegularExpressions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Genial.Cms.Domain.Aggregates;

public class CollectionField
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    [BsonIgnoreIfNull]
    public string Id { get; set; }

    [BsonElement("type")]
    public string Type { get; set; }

    [BsonElement("name")]
    public string Name { get; set; }

    [BsonElement("slug")]
    public string Slug { get; set; }

    [BsonElement("data")]
    public BsonDocument Data { get; set; }

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
        slug = Regex.Replace(slug, @"[^a-z0-9\s-]", "");

        // Substituir espaços por underscore
        slug = Regex.Replace(slug, @"\s+", "_");

        // Remover underscores múltiplos
        slug = Regex.Replace(slug, @"_+", "_");

        // Remover underscores no início e fim
        slug = slug.Trim('_');

        return slug;
    }
}
