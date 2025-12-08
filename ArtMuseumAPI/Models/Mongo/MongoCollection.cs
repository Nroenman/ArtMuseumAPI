namespace ArtMuseumAPI.Models.Mongo;

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class MongoCollection
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    public int OwnerId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }
}
