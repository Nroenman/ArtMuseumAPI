using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ArtMuseumAPI.Models.Mongo;

public class MongoArtist
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;
    public int? ArtistID { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Nationality { get; set; }
    public DateTime? BirthDate { get; set; }
    public DateTime? DeathDate { get; set; }
    public string? Biography { get; set; }
}