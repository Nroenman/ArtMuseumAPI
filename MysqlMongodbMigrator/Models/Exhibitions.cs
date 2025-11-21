using System.ComponentModel.DataAnnotations;

namespace MysqlMongodbMigrator.Models
{
    public class Exhibitions
    {
        [Key]
        public int ExhibitionID { get; set; }
        
        public string? Name { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? LocationID { get; set; }
        public Locations? Location { get; set; }
        public string? Description { get; set; }

        public ICollection<ExhibitionArtworks> ExhibitionArtworks { get; set; } = new List<ExhibitionArtworks>();
    }
}