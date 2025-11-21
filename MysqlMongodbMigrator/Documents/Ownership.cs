using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MysqlMongodbMigrator.Models.Mongo
{
    public class OwnershipDocument
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        public int? OwnershipID { get; set; }
        
        public int? ArtworkID { get; set; }
        public int? OwnerID { get; set; }
        public DateTime? AcquiredDate { get; set; }
        public DateTime? RelinquishedDate { get; set; }
        public string? SourceDocument { get; set; }
        public string? Notes { get; set; }
    }
}