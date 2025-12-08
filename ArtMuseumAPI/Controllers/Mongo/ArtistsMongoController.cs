using ArtMuseumAPI.Models.Mongo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace ArtMuseumAPI.Controllers.Mongo;

[ApiController]
[Route("api/mongo/[controller]")]
[ApiExplorerSettings(GroupName = "Mongo")]
public class ArtistsMongoController : ControllerBase
{
    private readonly IMongoCollection<MongoArtist> _artists;

    public ArtistsMongoController(IMongoClient client, IOptions<MongoSettings> mongoOptions)
    {
        var database = client.GetDatabase(mongoOptions.Value.DatabaseName);
        _artists = database.GetCollection<MongoArtist>("Artists");
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var artist = await _artists.Find(a => a.Id == id).FirstOrDefaultAsync();
        if (artist == null)
        {
            return NotFound();
        }

        return Ok(new
        {
            artist.Id,
            artist.FullName,
            artist.Nationality,
            artist.BirthDate,
            artist.DeathDate,
            artist.Biography
        });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] MongoArtist request)
    {
        if (request is null)
            return BadRequest("Request body is required.");
        
        if (string.IsNullOrWhiteSpace(request.FullName))
            return BadRequest("FullName is required.");

        var doc = new MongoArtist
        {
            FullName = request.FullName,
            Nationality = request.Nationality,
            BirthDate = request.BirthDate,
            DeathDate = request.DeathDate,
            Biography = request.Biography
        };
        await _artists.InsertOneAsync(doc);
        return CreatedAtAction(nameof(GetById), new { id = doc.Id }, new
        {
            doc.FullName,
            doc.Nationality,
            doc.BirthDate,
            doc.DeathDate,
            doc.Biography
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] MongoArtist request)
    {
        var update = Builders<MongoArtist>.Update
            .Set(a => a.FullName, request.FullName)
            .Set(a => a.Nationality, request.Nationality)
            .Set(a => a.BirthDate, request.BirthDate)
            .Set(a => a.DeathDate, request.DeathDate)
            .Set(a => a.Biography, request.Biography);
        
        var result = await _artists.UpdateOneAsync(a => a.Id == id, update);
        
        if (result.MatchedCount == 0)
            return NotFound();
        
        var updated = await _artists.Find(a => a.Id == id).FirstOrDefaultAsync();
        return Ok(new
        {
            updated!.Id,
            updated.FullName,
            updated.Nationality,
            updated.BirthDate,
            updated.DeathDate,
            updated.Biography
        });
    }
    
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var result = await _artists.DeleteOneAsync(c => c.Id == id);

        if (result.DeletedCount == 0)
            return NotFound();

        return NoContent();
    }
}