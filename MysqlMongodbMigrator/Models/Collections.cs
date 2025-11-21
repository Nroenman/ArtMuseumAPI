using System.ComponentModel.DataAnnotations;

namespace MysqlMongodbMigrator.Models
{
    public class Collections
    {
        [Key]
        public int CollectionID { get; set; }
        
        public string? Name { get; set; }
        public string? Description { get; set; }
        public int OwnerID { get; set; }
        public Owners? Owner { get; set; }

        public ICollection<CollectionItems> CollectionItems { get; set; } = new List<CollectionItems>();
    }
}