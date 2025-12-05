using ArtMuseumAPI.DTO;
using ArtMuseumAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArtMuseumAPI.Controllers
{
    [ApiController]
    [Route("api/mysql/[controller]")]
    [ApiExplorerSettings(GroupName = "MySql")]
    public class CollectionsController : ControllerBase
    {

        private readonly ApplicationDbContext _db;

        public CollectionsController(ApplicationDbContext db)
        {
            _db = db;
        }


        [Authorize(Roles = "Admin")]
        [HttpDelete("{id:int}")]
        public async Task<ActionResult<IEnumerable<object>>> DeleteController(int id)
        {

            var exists = await _db.Collections.AnyAsync(c => c.CollectionId == id);
            if (!exists)
                return NotFound();

            await _db.Database.ExecuteSqlRawAsync("CALL delete_collection({0})", id);


            return NoContent();
        }


        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetCollectionId(int id)
        {
            var collection = await _db.Collections
                .Where(c => c.CollectionId == id)
                .Select(c => new
                {
                    c.CollectionId,
                    c.OwnerId,
                    c.Name,
                    c.Description

                })
                .FirstOrDefaultAsync();

            return collection is null ? NotFound() : Ok(collection);
        }

        [HttpPost]
        public async Task<IActionResult> CreateCollection([FromBody] AddCollectionRequest request)
        {
            if (request is null)
                return BadRequest("Request body is required.");

            if (request.Name.Length >255)
                return BadRequest("Name is too long. It must be less than 255 characters long");


            var collection = new Collection
            {
                Name = request.Name,
                Description = request.Description,
                OwnerId = request.Owner
            };

            _db.Collections.Add(collection);

            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCollectionId), new { id = collection.CollectionId }, new
            {
                collection.CollectionId,
                collection.OwnerId,
                collection.Name,
                collection.Description
            });
        }

        [HttpPut("{id:int}/{ownerId:int}")]
        public async Task<IActionResult> UpdateOwnerOnCollection(int id,int ownerId)
        {
            var collection = await _db.Collections
                    .FirstOrDefaultAsync(c => c.CollectionId == id);

            if (collection == null)
            {
                return NotFound();
            }

            collection.OwnerId = ownerId;

            await _db.SaveChangesAsync();

            return Ok(collection);
        }
    }
}
