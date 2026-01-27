using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Genial.Cms.Domain.Aggregates;

public class Configuration
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    [BsonElement("key")]
    public string Key { get; set; }

    [BsonElement("status")]
    public bool Status { get; set; }

    [BsonElement("values")]
    public BsonDocument Values { get; set; }

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; }

    [BsonElement("updatedAt")]
    public DateTime UpdatedAt { get; set; }
}
