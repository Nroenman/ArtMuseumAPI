using System.ComponentModel.DataAnnotations;

namespace MysqlMongodbMigrator.Models
{
    public class Ownership
    {
        [Key]
        public int OwnershipID { get; set; }
        
        public int? ArtworkID { get; set; }
        public Artworks? Artwork { get; set; }
        public int OwnerID { get; set; }
        public Owners? Owner { get; set; }
        public DateTime? AcquiredDate { get; set; }
        public DateTime? RelinquishedDate { get; set; }
        public string? SourceDocument { get; set; }
        public string? Notes { get; set; }
    }
}