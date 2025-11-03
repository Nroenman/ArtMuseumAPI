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
                return null;  //return null hvis email/kode er forkert
            }

            return new User
            {
                UserId = user.UserId,
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
            var jwtToken = jwtHandler.ReadToken(token) as JwtSecurityToken;

            if (jwtToken == null)
                throw new UnauthorizedAccessException("Invalid token");

            var userName = jwtToken?.Claims?.FirstOrDefault(c => c.Type == "sub")?.Value;

            if (string.IsNullOrEmpty(userName))
                throw new UnauthorizedAccessException("Invalid token");

            var user = _context.Users.FirstOrDefault(u => u.UserName == userName);

            if (user == null)
                throw new UnauthorizedAccessException("User not found");

            return new User
            {
                UserId = user.UserId,
                UserName = user.UserName,
                Email = user.Email,
                Roles = user.Roles,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt
            };
        }
    }
