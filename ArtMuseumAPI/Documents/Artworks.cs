using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ArtMuseumAPI.Documents
{
    public class ArtworksDocument
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public int? ArtworkID { get; set; }
        public string? Title { get; set; }
        public string? Medium { get; set; }
        public int? YearCreated { get; set; }
        public string? Dimensions { get; set; }
        public int PrimaryArtistID { get; set; }
        public int CurrentLocationID { get; set; }
        public int CurrentOwnerID { get; set; }
        public string? Notes { get; set; }
    }
}