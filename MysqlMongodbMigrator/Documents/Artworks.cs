using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MysqlMongodbMigrator.Models.Mongo
{
    public class ArtworksDocument
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public int? ArtworkID { get; set; }
        public int? ArtistID { get; set; }
        public string? Title { get; set; }
        public int? YearCreated { get; set; }
        public string? Medium { get; set; }
        public string? Dimensions { get; set; }
    }
}