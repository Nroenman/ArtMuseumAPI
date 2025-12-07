using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ArtMuseumAPI.Documents
{
    public class LocationsDocument
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public int LocationID { get; set; }
        public string? Name { get; set; }
        public string? Address { get; set; }
        public string? Room { get; set; }
        public string? Shelf { get; set; }
    }
}