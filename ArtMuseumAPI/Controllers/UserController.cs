using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ArtMuseumAPI.Models;
using Microsoft.AspNetCore.Authorization;

namespace ArtMuseumAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UsersController(ApplicationDbContext context, IConfiguration configuration) : ControllerBase
{
    private readonly IConfiguration _configuration = configuration;

    // GET: api/Users

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetAllUsers()
    {
        var users = await context.Users
            .Select(u => new
            {
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
}
