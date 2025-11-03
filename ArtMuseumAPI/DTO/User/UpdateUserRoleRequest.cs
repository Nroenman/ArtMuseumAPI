namespace ArtMuseumAPI.DTO.User;

public class UpdateUserRoleRequest
{
    public string Email { get; set; } = default!;
    public string NewRole { get; set; } = default!;
}