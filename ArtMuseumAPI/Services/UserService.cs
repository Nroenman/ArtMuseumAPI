using System.IdentityModel.Tokens.Jwt;
using ArtMuseumAPI.Models;
using Microsoft.EntityFrameworkCore;
using Neo4j.Driver;

namespace ArtMuseumAPI.Services
{
    public class UserService(ApplicationDbContext db, IDriver neo4J) : IUserService
    {
        public User? Authenticate(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                return null;

            var normalizedEmail = email.Trim().ToLowerInvariant();

            var sqlUser = db.Users
                .AsNoTracking()
                .SingleOrDefault(u => u.Email == normalizedEmail);

            if (sqlUser != null && BCrypt.Net.BCrypt.Verify(password, sqlUser.PasswordHash))
            {
                return sqlUser;
            }

            using var session = neo4J.AsyncSession();

            var cursor = session
                .RunAsync(
                    @"MATCH (u:User { email: $email })
                      RETURN
                          u.userId       AS UserId,
                          u.userName     AS UserName,
                          u.email        AS Email,
                          u.passwordHash AS PasswordHash,
                          u.roles        AS Roles",
                    new { email = normalizedEmail })
                .GetAwaiter().GetResult();

            var list = cursor.ToListAsync().GetAwaiter().GetResult();
            var record = list.SingleOrDefault();

            if (record == null)
                return null;

            var passwordHash = record["PasswordHash"].As<string>();
            if (!BCrypt.Net.BCrypt.Verify(password, passwordHash))
                return null;

            var roles = record["Roles"].As<string?>();
            var userId = record["UserId"].As<int>();

            var neoUser = new User
            {
                UserId       = userId,
                UserName     = record["UserName"].As<string>(),
                Email        = record["Email"].As<string>(),
                PasswordHash = passwordHash,
                Roles        = roles ?? string.Empty,
            };

            return neoUser;
        }

        public User GetUserFromJwtToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                throw new UnauthorizedAccessException("Missing token");

            JwtSecurityToken jwt;
            try
            {
                var handler = new JwtSecurityTokenHandler();
                jwt = handler.ReadJwtToken(token);
            }
            catch
            {
                throw new UnauthorizedAccessException("Invalid token");
            }

            var sub = jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value;
            if (!int.TryParse(sub, out var userId))
                throw new UnauthorizedAccessException("Invalid token");

            var sqlUser = db.Users.AsNoTracking().SingleOrDefault(u => u.UserId == userId);
            if (sqlUser != null)
                return sqlUser;

            using var session = neo4J.AsyncSession();

            var cursor = session
                .RunAsync(
                    @"MATCH (u:User { userId: $uid })
                      RETURN
                          u.userId       AS UserId,
                          u.userName     AS UserName,
                          u.email        AS Email,
                          u.passwordHash AS PasswordHash,
                          u.roles        AS Roles",
                    new { uid = userId })
                .GetAwaiter().GetResult();

            var list = cursor.ToListAsync().GetAwaiter().GetResult();
            var record = list.SingleOrDefault();

            if (record == null)
                throw new UnauthorizedAccessException("User not found for token");

            return new User
            {
                UserId       = record["UserId"].As<int>(),
                UserName     = record["UserName"].As<string>(),
                Email        = record["Email"].As<string>(),
                PasswordHash = record["PasswordHash"].As<string>(),
                Roles        = record["Roles"].As<string?>() ?? ""
            };
        }
    }
}
