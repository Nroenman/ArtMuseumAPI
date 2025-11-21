using System.ComponentModel.DataAnnotations;

namespace MysqlMongodbMigrator.Models
{
    public class Locations
    {
        [Key]
        public int LocationID { get; set; }
        
        public string? Name { get; set; }
        public string? Address { get; set; }
        public string? Room { get; set; }
        public string? Shelf { get; set; }
    }
}