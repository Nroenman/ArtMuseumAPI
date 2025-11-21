using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MysqlMongodbMigrator.Models.Mongo
{
    public class CollectionItemsDocument
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public int? CollectionID { get; set; }
        public int? ArtworkID { get; set; }
        public DateTime? DateAdded { get; set; }
        public string? ItemNotes { get; set; }
    }
}