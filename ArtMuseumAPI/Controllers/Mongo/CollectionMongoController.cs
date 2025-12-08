using ArtMuseumAPI.DTO;
using ArtMuseumAPI.Models.Mongo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace ArtMuseumAPI.Controllers.Mongo;

[ApiController]
[Route("api/mongo/[controller]")]
[ApiExplorerSettings(GroupName = "Mongo")]
public class CollectionsMongoController : ControllerBase
{
    private readonly IMongoCollection<MongoCollection> _collections;

    public CollectionsMongoController(IMongoClient client, IOptions<MongoSettings> mongoOptions)
    {
        var db = client.GetDatabase(mongoOptions.Value.DatabaseName);
        _collections = db.GetCollection<MongoCollection>("Collections");
    }

    // GET: api/mongo/Collections/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var collection = await _collections
            .Find(c => c.Id == id)
            .FirstOrDefaultAsync();

        if (collection == null)
            return NotFound();

        return Ok(new
        {
            collection.Id,
            collection.OwnerId,
            collection.Name,
            collection.Description
        });
    }

    // POST: api/mongo/Collections
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] AddCollectionRequest request)
    {
        if (request is null)
            return BadRequest("Request body is required.");

        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest("Name is required.");

        if (request.Name.Length > 255)
            return BadRequest("Name is too long. It must be less than 255 characters long.");

        var doc = new MongoCollection
        {
            Name = request.Name,
            Description = request.Description,
            OwnerId = request.Owner
        };

        await _collections.InsertOneAsync(doc);

        return CreatedAtAction(nameof(GetById), new { id = doc.Id }, new
        {
            doc.Id,
            doc.OwnerId,
            doc.Name,
            doc.Description
        });
    }

    // PUT: api/mongo/Collections/{id}/owner/{ownerId}
    [HttpPut("{id}/owner/{ownerId:int}")]
    public async Task<IActionResult> UpdateOwner(string id, int ownerId)
    {
        var update = Builders<MongoCollection>.Update.Set(c => c.OwnerId, ownerId);

        var result = await _collections.UpdateOneAsync(c => c.Id == id, update);

        if (result.MatchedCount == 0)
            return NotFound();

        var updated = await _collections.Find(c => c.Id == id).FirstOrDefaultAsync();

        return Ok(new
        {
            updated!.Id,
            updated.OwnerId,
            updated.Name,
            updated.Description
        });
    }

    // DELETE: api/mongo/Collections/{id}
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var result = await _collections.DeleteOneAsync(c => c.Id == id);

        if (result.DeletedCount == 0)
            return NotFound();

        return NoContent();
    }
}
