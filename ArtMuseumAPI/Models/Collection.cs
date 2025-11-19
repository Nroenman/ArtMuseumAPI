namespace ArtMuseumAPI.Models
{
    public class Collection
    {
        public int CollectionId { get; set; }

        public string Name { get; set; } = null!;

        public string? Description { get; set; }

        public int? OwnerId { get; set; }

 
    }

}
