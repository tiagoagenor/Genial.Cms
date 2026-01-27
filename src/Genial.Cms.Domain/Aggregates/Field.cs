using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Genial.Cms.Domain.Aggregates;

public class Field
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    [BsonElement("key")]
    public string Key { get; set; }

    [BsonElement("label")]
    public string Label { get; set; }

    [BsonElement("type")]
    public string Type { get; set; }

    [BsonElement("icon")]
    public string Icon { get; set; }

    [BsonElement("active")]
    public bool Active { get; set; }

    [BsonElement("order")]
    public int Order { get; set; }

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; }

    [BsonElement("updatedAt")]
    public DateTime UpdatedAt { get; set; }
}

