using System.IdentityModel.Tokens.Jwt;
using ArtMuseumAPI.Models;

namespace ArtMuseumAPI.Services;

public class UserService(ApplicationDbContext context, IConfiguration configuration) : IUserService
{
    private readonly ApplicationDbContext _context = context;
    private readonly IConfiguration _configuration = configuration;

    public User Authenticate(string email, string password)
    {
        var user = _context.Users.FirstOrDefault(u => u.Email == email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
        {
            return null; //return null hvis email/kode er forkert
        }

        return new User
        {
            UserName = user.UserName,
            Email = user.Email,
            Roles = user.Roles,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };
    }

    public User GetUserFromJwtToken(string token)
    {
        var jwtHandler = new JwtSecurityTokenHandler();
        var jwtToken = jwtHandler.ReadToken(token) as JwtSecurityToken
                       ?? throw new UnauthorizedAccessException("Invalid token");

        var sub = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value
                  ?? throw new UnauthorizedAccessException("Invalid token");

        if (!int.TryParse(sub, out var userId))
            throw new UnauthorizedAccessException("Invalid subject claim");

        var user = _context.Users.FirstOrDefault(u => u.UserId == userId)
                   ?? throw new UnauthorizedAccessException("User not found");

        return new User
        {
            UserName = user.UserName,
            Email = user.Email,
            Roles = user.Roles,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };
    }
}
