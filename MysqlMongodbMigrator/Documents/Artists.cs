using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MysqlMongodbMigrator.Models.Mongo
{
    public class ArtistsDocument
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public int? ArtistID { get; set; }
        public string? FullName { get; set; }
        public string? Nationality { get; set; }
        public DateTime? BirthDate { get; set; }
        public DateTime? DeathDate { get; set; }
        public string? Biography { get; set; }
    }
}