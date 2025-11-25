using ArtMuseumAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;

namespace ArtMuseumAPI.Controllers.Mysql
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
    }
}
