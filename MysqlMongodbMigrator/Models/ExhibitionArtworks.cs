using System.ComponentModel.DataAnnotations;

namespace MysqlMongodbMigrator.Models
{
    public class ExhibitionArtworks
    {
        [Key]
        public int ExhibitionID { get; set; }
        
        public Exhibitions? Exhibition { get; set; }
        public int ArtworkID { get; set; }
        public Artworks? Artwork { get; set; }
        public string? DisplayLabel { get; set; }
        public string? Notes { get; set; }
    }
}