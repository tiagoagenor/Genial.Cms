using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Genial.Cms.Domain.Aggregates;

public class TypeFile
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    [BsonElement("key")]
    public string Key { get; set; }

    [BsonElement("value")]
    public string Value { get; set; }

    [BsonElement("order")]
    public int Order { get; set; }

    [BsonElement("category")]
    public string Category { get; set; }

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; }

    [BsonElement("updatedAt")]
    public DateTime UpdatedAt { get; set; }
}
