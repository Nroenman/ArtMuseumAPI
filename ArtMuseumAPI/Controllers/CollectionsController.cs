using ArtMuseumAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;

namespace ArtMuseumAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CollectionsController : Controller
    {

        private readonly ApplicationDbContext _db;

        public CollectionsController(ApplicationDbContext db)
        {
            _db = db;
        }

        // GET: api/Users (Admin only) — adjust/remove Roles if not present
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id:int}")]
        public async Task<ActionResult<IEnumerable<object>>> DeleteController(int id)
        {
            // Optional: you can first check if it exists
            var exists = await _db.Collections.AnyAsync(c => c.CollectionId == id);
            if (!exists)
                return NotFound();
            
            // Call your stored procedure
            await _db.Database.ExecuteSqlRawAsync("CALL delete_collection({0})", id);

            // or if your MySQL provider prefers INTERPOLATED:
            // await _db.Database.ExecuteSqlInterpolatedAsync($"CALL delete_collection({id})");

            return NoContent();
        }
    }
}
