using System.ComponentModel.DataAnnotations;

namespace MysqlMongodbMigrator.Models
{
    public class Owners
    {
        [Key]
        public int OwnerID { get; set; }
        
        public string? Name { get; set; }
        public string? OwnerType { get; set; }
        public string? ContactEmail { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }

        public ICollection<Ownership> Ownerships { get; set; } = new List<Ownership>();
        public ICollection<Transactions> TransactionsFrom { get; set; } = new List<Transactions>();
        public ICollection<Transactions> TransactionsTo { get; set; } = new List<Transactions>();
    }
}