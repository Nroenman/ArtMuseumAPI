using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ArtMuseumAPI.Documents
{
    public class ExhibitionsDocument
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public int? ExhibitionID { get; set; }
        public string? Name { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Description { get; set; }
        public int? LocationID { get; set; }
    }
}