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
public class ArtistsMongoController : ControllerBase
{
    private readonly IMongoCollection<MongoArtist> _artists;

    public ArtistsMongoController(IMongoClient client, IOptions<MongoSettings> mongoOptions)
    {
        var database = client.GetDatabase(mongoOptions.Value.DatabaseName);
        _artists = database.GetCollection<MongoArtist>("Artists");
    }

    [HttpGet("by-artistid/{artistId:int}")]
    public async Task<IActionResult> GetByArtistId(int artistId)
    {
        var filter = Builders<MongoArtist>.Filter.Eq(a => a.ArtistID, artistId);
        var results = await _artists.Find(filter).ToListAsync();

        if (results == null || results.Count == 0)
            return NotFound();

        return Ok(results.Select(a => new
        {
            a.Id,
            a.ArtistID,
            a.FullName,
            a.Nationality,
            a.BirthDate,
            a.DeathDate,
            a.Biography
        }));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] AddArtistRequest request)
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
    
        return Ok(new
        {
            doc.Id,
            doc.ArtistID,
            doc.FullName,
            doc.Nationality,
            doc.BirthDate,
            doc.DeathDate,
            doc.Biography
        });
    }


    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] AddArtistRequest request)
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
    
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var result = await _artists.DeleteOneAsync(c => c.Id == id);

        if (result.DeletedCount == 0)
            return NotFound();

        return NoContent();
    }
}