using System.ComponentModel.DataAnnotations;

namespace MysqlMongodbMigrator.Models
{
    public class Transactions
    {
        [Key]
        public int TransactionID { get; set; }
        
        public int? ArtworkID { get; set; }
        public Artworks? Artwork { get; set; }
        public DateTime TxnDate { get; set; }
        public string? TxnType { get; set; }
        public int? FromOwnerID { get; set; }
        public Owners? FromOwner { get; set; }
        public int? ToOwnerID { get; set; }
        public Owners? ToOwner { get; set; }
        public int? Price { get; set; }
        public string? Currency { get; set; }
        public string? Notes { get; set; }
    }
}