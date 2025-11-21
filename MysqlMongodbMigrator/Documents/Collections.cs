using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MysqlMongodbMigrator.Models.Mongo
{
    public class CollectionsDocument
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public int? CollectionID { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public int? OwnerID { get; set; }
    }
}