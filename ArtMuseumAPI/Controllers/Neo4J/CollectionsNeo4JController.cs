using System.Linq;
using ArtMuseumAPI.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Neo4j.Driver;

namespace ArtMuseumAPI.Controllers.Neo4J;

[ApiController]
[Route("api/neo4j/[controller]")]
[ApiExplorerSettings(GroupName = "Neo4j")]
public class CollectionsNeo4JController(IDriver driver) : ControllerBase
{
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        await using var session = driver.AsyncSession();

        var cursor = await session.RunAsync(
            @"MATCH (c:Collection { collectionId: $id })
              RETURN
                  c.collectionId AS CollectionId,
                  c.name         AS Name,
                  c.description  AS Description",
            new { id });

        var records = await cursor.ToListAsync();
        var record = records.SingleOrDefault();

        if (record == null)
            return NotFound();

        return Ok(new
        {
            CollectionId = record["CollectionId"].As<int>(),
            Name         = record["Name"].As<string>(),
            Description  = record["Description"].As<string?>()
        });
    }
    
    [HttpGet]
    public async Task<IActionResult> GetSample()
    {
        await using var session = driver.AsyncSession();

        var cursor = await session.RunAsync(
            @"MATCH (c:Collection)
              RETURN
                  c.collectionId AS CollectionId,
                  c.name         AS Name,
                  c.description  AS Description
              LIMIT 10");

        var records = await cursor.ToListAsync();

        var list = records.Select(r => new
        {
            CollectionId = r["CollectionId"].As<int>(),
            Name         = r["Name"].As<string>(),
            Description  = r["Description"].As<string?>()
        });

        return Ok(list);
    }
    
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] AddCollectionRequest request)
    {
        if (request is null)
            return BadRequest("Request body is required.");

        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest("Name is required.");

        if (request.Name.Length > 255)
            return BadRequest("Name is too long. It must be less than 255 characters long.");

        await using var session = driver.AsyncSession();

        var cursor = await session.RunAsync(
            @"
            // Calculate next collectionId
            CALL {
                MATCH (c:Collection)
                RETURN coalesce(max(c.collectionId), 0) + 1 AS nextId
            }
            CREATE (c:Collection {
                collectionId: nextId,
                name: $name,
                description: $description
            })
            RETURN
                c.collectionId AS CollectionId,
                c.name         AS Name,
                c.description  AS Description
            ",
            new
            {
                name        = request.Name,
                description = request.Description ?? string.Empty
            });

        var record = (await cursor.ToListAsync()).Single();

        var newId = record["CollectionId"].As<int>();

        return CreatedAtAction(nameof(GetById), new { id = newId }, new
        {
            CollectionId = newId,
            Name         = record["Name"].As<string>(),
            Description  = record["Description"].As<string?>()
        });
    }

    [HttpPut("{id:int}/owner/{ownerId:int}")]
    public async Task<IActionResult> UpdateOwner(int id, int ownerId)
    {
        await using var session = driver.AsyncSession();

        var cursor = await session.RunAsync(
            @"MATCH (c:Collection { collectionId: $id })
              SET c.ownerId = $ownerId
              RETURN
                  c.collectionId AS CollectionId,
                  c.ownerId      AS OwnerId,
                  c.name         AS Name,
                  c.description  AS Description",
            new { id, ownerId });

        var records = await cursor.ToListAsync();
        var record = records.SingleOrDefault();

        if (record == null)
            return NotFound();

        return Ok(new
        {
            CollectionId = record["CollectionId"].As<int>(),
            OwnerId      = record["OwnerId"].As<int>(),
            Name         = record["Name"].As<string>(),
            Description  = record["Description"].As<string?>()
        });
    }
    
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await using var session = driver.AsyncSession();

        var cursor = await session.RunAsync(
            @"MATCH (c:Collection { collectionId: $id })
              DETACH DELETE c
              RETURN COUNT(c) AS Deleted",
            new { id });

        var record = (await cursor.ToListAsync()).Single();
        var deleted = record["Deleted"].As<long>();

        if (deleted == 0)
            return NotFound();

        return NoContent();
    }
}
