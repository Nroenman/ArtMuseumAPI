using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MysqlMongodbMigrator.Models.Mongo
{
    public class ExhibitionArtworksDocument
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public int ExhibitionID { get; set; }
        public int? ArtworkID { get; set; }
        public string? DisplayLabel { get; set; }
        public string? Notes { get; set; }
    }
}