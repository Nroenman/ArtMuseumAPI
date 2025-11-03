namespace ArtMuseumAPI.DTO.User;

public class UserRegisterRequest
{
    public string? UserName { get; set; } 
    public string Email { get; set; } = string.Empty;  // required
    public string Password { get; set; } = string.Empty;  // required
}