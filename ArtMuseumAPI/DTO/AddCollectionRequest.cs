namespace ArtMuseumAPI.DTO
{
    public class AddCollectionRequest
    {
        public string Name { get; set; } = string.Empty;  // required
        public string? Description { get; set; } = string.Empty;  // required
        public int Owner { get; set; } // required
    }
}
