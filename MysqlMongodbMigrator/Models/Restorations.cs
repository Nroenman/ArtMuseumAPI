using System.ComponentModel.DataAnnotations;

namespace MysqlMongodbMigrator.Models
{
    public class Restorations
    {
        [Key]
        public int RestorationID { get; set; }
        
        public int? ArtworkID { get; set; }
        public Artworks? Artwork { get; set; }
        public DateTime RestorationDate { get; set; }
        public string? Conservator { get; set; }
        public string? RestorationType { get; set; }
        public string? Details { get; set; }
        public string? ConditionBefore { get; set; }
        public string? ConditionAfter { get; set; }
        public int? Cost { get; set; }
        public string? Currency { get; set; }
    }
}