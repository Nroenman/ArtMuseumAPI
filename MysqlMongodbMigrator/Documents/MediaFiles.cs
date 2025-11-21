using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MysqlMongodbMigrator.Models.Mongo
{
    public class MediaFilesDocument
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public int? MediaFileID { get; set; }
        public int? ArtworkID { get; set; }
        public Artworks? Artwork { get; set; }
        public int? ArtistID { get; set; }
        public Artists? Artist { get; set; }
        public string? MediaType { get; set; }
        public string? Title { get; set; }
        public string? FileURL { get; set; }
        public DateTime? CapturedDate { get; set; }
        public string? CopyrightHolder { get; set; }
        public string? Notes { get; set; }
    }
}