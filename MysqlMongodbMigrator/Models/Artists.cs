using System.ComponentModel.DataAnnotations;

namespace MysqlMongodbMigrator.Models
{
    public class Artists
    {
        [Key]
        public int ArtistID { get; set; }
        

        public string? FullName { get; set; }

        public string? Nationality { get; set; }

        public DateTime? BirthDate { get; set; }

        public DateTime? DeathDate { get; set; }

        public string? Biography { get; set; }
    }
}