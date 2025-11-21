using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MysqlMongodbMigrator.Models.Mongo
{
    public class TransactionsDocument
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        public int TransactionID { get; set; }
        
        public int? ArtworkID { get; set; }
        public DateTime TxnDate { get; set; }
        public string? TxnType { get; set; }
        public int? FromOwnerID { get; set; }
        public int? ToOwnerID { get; set; }
        public int? Price { get; set; }
        public string? Currency { get; set; }
        public string? Notes { get; set; }
    }
}