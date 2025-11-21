using System.ComponentModel.DataAnnotations;

namespace MysqlMongodbMigrator.Models
{
    public class CollectionItems
    {
        [Key]
        public int CollectionID { get; set; }
        
        public Collections? Collection { get; set; }
        public int ArtworkID { get; set; }
        public Artworks? Artwork { get; set; }
        public DateTime? DateAdded { get; set; }
        public string? ItemNotes { get; set; }
    }
}