namespace ArtMuseumAPI.DTO;

public class AddArtistRequest
{
    public string FullName { get; set; } = string.Empty;
    public string? Nationality { get; set; }
    public DateTime BirthDate { get; set; }
    public DateTime? DeathDate { get; set; }
    public string? Biography { get; set; }
}