using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Genial.Cms.Domain.Aggregates;

public class CollectionItemChange
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    [BsonElement("collectionId")]
    public string CollectionId { get; set; }

    [BsonElement("itemId")]
    public string ItemId { get; set; }

    [BsonElement("userId")]
    public string UserId { get; set; }

    [BsonElement("changeType")]
    [BsonRepresentation(BsonType.Int32)]
    public int ChangeType { get; set; }

    [BsonElement("beforeData")]
    public BsonDocument? BeforeData { get; set; }

    [BsonElement("afterData")]
    public BsonDocument? AfterData { get; set; }

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; }
}
