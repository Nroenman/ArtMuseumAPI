using System.ComponentModel.DataAnnotations;

namespace MysqlMongodbMigrator.Models
{
    public class Artworks
    {
        [Key]
        public int ArtworkID { get; set; }
        
        public string? Title { get; set; }
        public string? Medium { get; set; }
        public int? YearCreated { get; set; }
        public string? Dimensions { get; set; }
        public int PrimaryArtistID { get; set; }
        public Artists? PrimaryArtist { get; set; }
        public int CurrentLocationID { get; set; }
        public Locations? CurrentLocation { get; set; }
        public int CurrentOwnerID { get; set; }
        public Owners? CurrentOwner { get; set; }
    }
}