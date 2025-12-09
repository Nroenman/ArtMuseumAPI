using ArtMuseumAPI.DTO;
using ArtMuseumAPI.Models.Mongo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace ArtMuseumAPI.Controllers.Mongo;

[ApiController]
// Explicit route so it matches exactly what Swagger calls
[Route("api/mongo/CollectionsMongo")]
[ApiExplorerSettings(GroupName = "Mongo")]
public class CollectionsMongoController : ControllerBase
{
    private readonly IMongoCollection<MongoCollection> _collections;

    public CollectionsMongoController(IMongoClient client, IOptions<MongoSettings> mongoOptions)
    {
        var db = client.GetDatabase(mongoOptions.Value.DatabaseName);
        _collections = db.GetCollection<MongoCollection>("Collections");
    }

    // ------------------------------------------------------------
    // GET: api/mongo/CollectionsMongo/{collectionId}
    // Look up by numeric CollectionID (NOT Mongo ObjectId)
    // ------------------------------------------------------------
    [HttpGet("{collectionId:int}")]
    public async Task<IActionResult> GetByCollectionId(int collectionId)
    {
        var collection = await _collections
            .Find(c => c.CollectionID == collectionId)
            .FirstOrDefaultAsync();

        if (collection == null)
            return NotFound();

        return Ok(new
        {
            collection.CollectionID,
            collection.OwnerID,
            collection.Name,
            collection.Description
        });
    }

    // ------------------------------------------------------------
    // POST: api/mongo/CollectionsMongo
    // Creates a new collection and auto-assigns CollectionID
    // ------------------------------------------------------------
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] AddCollectionRequest request)
    {
        if (request is null)
            return BadRequest("Request body is required.");

        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest("Name is required.");

        if (request.Name.Length > 255)
            return BadRequest("Name is too long (max 255 characters).");

        // Simple "next ID" based on count – good enough for demo
        var nextId = (int)(await _collections.CountDocumentsAsync(_ => true)) + 1;

        var doc = new MongoCollection
        {
            CollectionID = nextId,
            Name = request.Name,
            Description = request.Description,
            OwnerID = request.Owner
        };

        await _collections.InsertOneAsync(doc);

        return CreatedAtAction(
            nameof(GetByCollectionId),
            new { collectionId = doc.CollectionID },
            new
            {
                doc.CollectionID,
                doc.OwnerID,
                doc.Name,
                doc.Description
            });
    }

    // ------------------------------------------------------------
    // PUT: api/mongo/CollectionsMongo/{collectionId}/owner/{ownerId}
    // Update owner using numeric CollectionID
    // ------------------------------------------------------------
    [HttpPut("{collectionId:int}/owner/{ownerId:int}")]
    public async Task<IActionResult> UpdateOwner(int collectionId, int ownerId)
    {
        var update = Builders<MongoCollection>.Update.Set(c => c.OwnerID, ownerId);

        var result = await _collections.UpdateOneAsync(
            c => c.CollectionID == collectionId,
            update
        );

        if (result.MatchedCount == 0)
            return NotFound();

        var updated = await _collections
            .Find(c => c.CollectionID == collectionId)
            .FirstOrDefaultAsync();

        return Ok(new
        {
            updated!.CollectionID,
            updated.OwnerID,
            updated.Name,
            updated.Description
        });
    }

    // ------------------------------------------------------------
    // DELETE: api/mongo/CollectionsMongo/{collectionId}
    // ------------------------------------------------------------
    [Authorize(Roles = "Admin")]
    [HttpDelete("{collectionId:int}")]
    public async Task<IActionResult> Delete(int collectionId)
    {
        var result = await _collections.DeleteOneAsync(c => c.CollectionID == collectionId);

        if (result.DeletedCount == 0)
            return NotFound();

        return NoContent();
    }
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var docs = await _collections
            .Find(Builders<MongoCollection>.Filter.Empty)
            .ToListAsync();

        return Ok(docs.Select(c => new
        {
            c.Id,           // Mongo ObjectId
            c.CollectionID, // our numeric id
            c.OwnerID,
            c.Name,
            c.Description
        }));
    }

}
