using System.Linq;
using ArtMuseumAPI.DTO.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Neo4j.Driver;

namespace ArtMuseumAPI.Controllers.Neo4J;

[ApiController]
[Route("api/neo4j/[controller]")]
[ApiExplorerSettings(GroupName = "Neo4j")]
public class UsersNeo4JController(IDriver driver) : ControllerBase
{
    // GET: api/neo4j/UsersNeo4J  (Admin only)
    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetAllUsers()
    {
        await using var session = driver.AsyncSession();

        var cursor = await session.RunAsync(
            @"MATCH (u:User)
              RETURN
                  u.userId     AS UserId,
                  u.userName   AS UserName,
                  u.email      AS Email,
                  u.roles      AS Roles,
                  u.createdAt  AS CreatedAt,
                  u.updatedAt  AS UpdatedAt
              ORDER BY u.userId");

        var records = await cursor.ToListAsync();

        var users = records.Select(r => new
        {
            UserId    = r["UserId"].As<int>(),
            UserName  = r["UserName"].As<string>(),
            Email     = r["Email"].As<string>(),
            Roles     = r["Roles"].As<string?>(),
            CreatedAt = r["CreatedAt"].As<string?>(),
            UpdatedAt = r["UpdatedAt"].As<string?>()
        });

        return Ok(users);
    }

    // POST: api/neo4j/UsersNeo4J/register
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] UserRegisterRequest request)
    {
        if (request is null)
            return BadRequest("Request body is required.");

        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            return BadRequest("Email and Password are required.");

        var email = request.Email.Trim().ToLowerInvariant();

        await using var session = driver.AsyncSession();

        // 1) check if email already exists
        var existsCursor = await session.RunAsync(
            @"MATCH (u:User { email: $email })
              RETURN count(u) AS Cnt",
            new { email });

        var existsCount = (await existsCursor.SingleAsync())["Cnt"].As<long>();
        if (existsCount > 0)
            return Conflict("User already exists.");

        // 2) compute next userId
        var nextIdCursor = await session.RunAsync(
            @"MATCH (u:User)
              RETURN coalesce(max(u.userId), 0) + 1 AS NextId");

        var nextId = (await nextIdCursor.SingleAsync())["NextId"].As<int>();

        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);
        var now = DateTime.UtcNow.ToString("O"); // ISO 8601

        var userName = string.IsNullOrWhiteSpace(request.UserName)
            ? email.Split('@')[0]
            : request.UserName!.Trim();

        // default role, like MySQL ("user")
        const string defaultRoles = "user";

        // 3) create node
        var createCursor = await session.RunAsync(
            @"CREATE (u:User {
                    userId:       $userId,
                    userName:     $userName,
                    email:        $email,
                    passwordHash: $passwordHash,
                    roles:        $roles,
                    createdAt:    $createdAt,
                    updatedAt:    $updatedAt
               })
              RETURN
                  u.userId     AS UserId,
                  u.userName   AS UserName,
                  u.email      AS Email,
                  u.roles      AS Roles,
                  u.createdAt  AS CreatedAt,
                  u.updatedAt  AS UpdatedAt",
            new
            {
                userId       = nextId,
                userName,
                email,
                passwordHash = hashedPassword,
                roles        = defaultRoles,
                createdAt    = now,
                updatedAt    = now
            });

        var record = (await createCursor.ToListAsync()).Single();

        return CreatedAtAction(nameof(GetUserById), new { id = nextId }, new
        {
            UserId    = record["UserId"].As<int>(),
            UserName  = record["UserName"].As<string>(),
            Email     = record["Email"].As<string>(),
            Roles     = record["Roles"].As<string>(),
            CreatedAt = record["CreatedAt"].As<string>(),
            UpdatedAt = record["UpdatedAt"].As<string>()
        });
    }

    // GET: api/neo4j/UsersNeo4J/{id}
    [Authorize(Roles = "Admin")]
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetUserById(int id)
    {
        await using var session = driver.AsyncSession();

        var cursor = await session.RunAsync(
            @"MATCH (u:User { userId: $id })
              RETURN
                  u.userId     AS UserId,
                  u.userName   AS UserName,
                  u.email      AS Email,
                  u.roles      AS Roles,
                  u.createdAt  AS CreatedAt,
                  u.updatedAt  AS UpdatedAt",
            new { id });

        var records = await cursor.ToListAsync();
        var record = records.SingleOrDefault();

        if (record == null)
            return NotFound();

        return Ok(new
        {
            UserId    = record["UserId"].As<int>(),
            UserName  = record["UserName"].As<string>(),
            Email     = record["Email"].As<string>(),
            Roles     = record["Roles"].As<string?>(),
            CreatedAt = record["CreatedAt"].As<string?>(),
            UpdatedAt = record["UpdatedAt"].As<string?>()
        });
    }

    // PUT: api/neo4j/UsersNeo4J/{id}/roles
    // Allow changing roles (e.g. "Admin", "Admin,user", etc.)
    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}/roles")]
    public async Task<IActionResult> UpdateRoles(int id, [FromBody] string roles)
    {
        if (string.IsNullOrWhiteSpace(roles))
            return BadRequest("Roles string is required.");

        await using var session = driver.AsyncSession();

        var cursor = await session.RunAsync(
            @"MATCH (u:User { userId: $id })
              SET u.roles = $roles,
                  u.updatedAt = $updatedAt
              RETURN
                  u.userId     AS UserId,
                  u.userName   AS UserName,
                  u.email      AS Email,
                  u.roles      AS Roles,
                  u.createdAt  AS CreatedAt,
                  u.updatedAt  AS UpdatedAt",
            new
            {
                id,
                roles,
                updatedAt = DateTime.UtcNow.ToString("O")
            });

        var records = await cursor.ToListAsync();
        var record = records.SingleOrDefault();

        if (record == null)
            return NotFound();

        return Ok(new
        {
            UserId    = record["UserId"].As<int>(),
            UserName  = record["UserName"].As<string>(),
            Email     = record["Email"].As<string>(),
            Roles     = record["Roles"].As<string>(),
            CreatedAt = record["CreatedAt"].As<string?>(),
            UpdatedAt = record["UpdatedAt"].As<string?>()
        });
    }
}
