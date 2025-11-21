using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MysqlMongodbMigrator.Models.Mongo
{
    public class RestorationsDocument
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        public int RestorationID { get; set; }
        
        public int? ArtworkID { get; set; }
        public DateTime RestorationDate { get; set; }
        public string? Conservator { get; set; }
        public string? Details { get; set; }
        public string? ConditionBefore { get; set; }
        public string? ConditionAfter { get; set; }
        public int? Cost { get; set; }
        public string? Currency { get; set; }
    }
}