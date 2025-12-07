using ArtMuseumAPI.Documents;
using ArtMuseumAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace ArtMuseumAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ArtistsController : ControllerBase
{
    private readonly IArtistsService _artistsService;

    public ArtistsController(IArtistsService artistsService)
    {
        _artistsService = artistsService;
    }

    [HttpGet]
    public async Task<ActionResult<List<ArtistsDocument>>> GetAll()
    {
        return Ok(await _artistsService.GetAllAsync());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ArtistsDocument>> GetById(string id)
    {
        var artist = await _artistsService.GetByIdAsync(id);
        if (artist == null)
        {
            return NotFound();
        }
        return Ok(artist);
    }

    [HttpPost]
    public async Task<ActionResult> Create(ArtistsDocument artist)
    {
        await _artistsService.CreateAsync(artist);
        return CreatedAtAction(nameof(GetById), new { id = artist.Id }, artist);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(string id, ArtistsDocument artist)
    {
        var existing = await _artistsService.GetByIdAsync(id);
        if (existing == null)
            return NotFound();
            
        await _artistsService.UpdateAsync(id, artist);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(string id)
    {
        var existing = await _artistsService.GetByIdAsync(id);
        if (existing == null)
            return NotFound();
            
        await _artistsService.DeleteAsync(id);
        return NoContent();
    }
}