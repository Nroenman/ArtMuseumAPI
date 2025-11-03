using System.IdentityModel.Tokens.Jwt;
using ArtMuseumAPI.DTO.User;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using ArtMuseumAPI.Models;

namespace ArtMuseumAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController(ApplicationDbContext context, IConfiguration configuration) : ControllerBase
{
    private readonly IConfiguration _configuration = configuration;

    // GET: api/Users
    [Authorize(Roles = "Admin")] 
    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetAllUsers()
    {
        var users = await context.Users
            .Select(u => new {
                u.UserId,
                u.UserName,
                u.Email,
                u.CreatedAt,
                u.UpdatedAt,
                u.Roles
            })
            .ToListAsync();

        return Ok(users);
    }

    // GET: api/Users/{id}
    [HttpGet("{id:int}")]
    public async Task<ActionResult<object>> GetUserById(int id)
    {
        var user = await context.Users
            .Where(u => u.UserId == id)
            .Select(u => new {
                u.UserId,
                u.UserName,
                u.Email,
                u.CreatedAt,
                u.UpdatedAt,
                u.Roles
            })
            .FirstOrDefaultAsync();

        if (user is null) return NotFound();
        return Ok(user);
    }

    [HttpPost("register")]
    public async Task<ActionResult<object>> Register([FromBody] UserRegisterRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Email))
            return BadRequest("Email is required.");
        if (string.IsNullOrWhiteSpace(req.Password))
            return BadRequest("Password is required.");

        var exists = await context.Users.AnyAsync(u => u.Email == req.Email);
        if (exists)
            return BadRequest("Email is already in use.");

        var user = new User
        {
            UserName   = string.IsNullOrWhiteSpace(req.UserName) ? req.Email : req.UserName.Trim(),
            Email      = req.Email.Trim(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password),
            Roles      = "User",
            CreatedAt  = DateTime.UtcNow,
            UpdatedAt  = DateTime.UtcNow
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetUserById), new { id = user.UserId }, new {
            user.UserId,
            user.UserName,
            user.Email,
            user.CreatedAt,
            user.UpdatedAt,
            user.Roles
        });
    }
   
    [HttpPut("update-role")]
    public async Task<IActionResult> UpdateUserRole([FromBody] UpdateUserRoleRequest request)
    {
        // validate input
        if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.NewRole))
            return BadRequest(new { Message = "Email and new role are required." });

        // find user by email
        var user = await context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (user == null)
            return NotFound(new { Message = "User not found." });

        // optionally validate allowed roles
        var validRoles = new[] { "Admin", "User" };
        if (!validRoles.Contains(request.NewRole))
            return BadRequest(new { Message = "Invalid role. Allowed roles: Admin, User" });

        // update role
        user.Roles = request.NewRole;
        user.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync();

        return Ok(new { Message = $"User '{user.Email}' role updated to '{user.Roles}'" });
    }


    // GET: api/Users/profile
    [Authorize]
    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        var token = Request.Headers["Authorization"].FirstOrDefault()?.Split(' ').Last();
        if (string.IsNullOrEmpty(token))
            return Unauthorized(new { Message = "No token provided." });

        try
        {
            var profile = await GetUserProfileFromToken(token);
            if (profile is null)
                return Unauthorized(new { Message = "Invalid or expired token." });

            if (!int.TryParse(profile.UserId, out var userId))
                return Unauthorized(new { Message = "Invalid token subject." });

            var user = await context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user is null) return NotFound(new { Message = "User not found." });

            return Ok(new {
                user.UserId,
                user.UserName,
                user.Email,
                user.CreatedAt,
                user.UpdatedAt,
                user.Roles
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "Error retrieving user profile", Details = ex.Message });
        }
    }

    private Task<UserProfile?> GetUserProfileFromToken(string token)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadToken(token) as JwtSecurityToken;
            var sub = jwt?.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
            if (string.IsNullOrEmpty(sub)) return Task.FromResult<UserProfile?>(null);
            return Task.FromResult<UserProfile?>(new UserProfile { UserId = sub });
        }
        catch
        {
            return Task.FromResult<UserProfile?>(null);
        }
        
    }

    private class UserProfile { public string UserId { get; set; } = default!; }
}
