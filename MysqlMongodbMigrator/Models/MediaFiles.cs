using System.ComponentModel.DataAnnotations;

namespace MysqlMongodbMigrator.Models
{
    public class MediaFiles
    {
        [Key]
        public int MediaFileID { get; set; }
        
        public int? ArtworkID { get; set; }
        public Artworks? Artwork { get; set; }
        public int? ArtistID { get; set; }
        public Artists? Artist { get; set; }
        public string? MediaType { get; set; }
        public string? Title { get; set; }
        public string? FileURL { get; set; }
        public DateTime? CapturedDate { get; set; }
        public string? CopyrightHolder { get; set; }
        public string? Notes { get; set; }
    }
}