using ArtMuseumAPI.DTO;
using ArtMuseumAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArtMuseumAPI.Controllers.Sql
{
    [ApiController]
    [Route("api/mysql/[controller]")]
    [ApiExplorerSettings(GroupName = "MySql")]
    public class CollectionsController(ICollectionsService collectionsService) : ControllerBase
    {
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteController(int id)
        {
            var deleted = await collectionsService.DeleteCollectionAsync(id);
            if (!deleted)
                return NotFound();

            return Ok(new
            {
                message = "Collection deleted",
                collectionId = id
            });
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetCollectionId(int id)
        {
            var collection = await collectionsService.GetCollectionAsync(id);
            if (collection is null)
                return NotFound();

            return Ok(new
            {
                collection.CollectionId,
                collection.OwnerId,
                collection.Name,
                collection.Description
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateCollection([FromBody] AddCollectionRequest request)
        {
            if (request is null)
                return BadRequest("Request body is required.");

            if (request.Name.Length > 255)
                return BadRequest("Name is too long. It must be less than 255 characters long");

            var collection = await collectionsService.CreateCollectionAsync(request);

            return CreatedAtAction(nameof(GetCollectionId), new { id = collection.CollectionId }, new
            {
                collection.CollectionId,
                collection.OwnerId,
                collection.Name,
                collection.Description
            });
        }

        [HttpPut("{id:int}/{ownerId:int}")]
        public async Task<IActionResult> UpdateOwnerOnCollection(int id, int ownerId)
        {
            var updated = await collectionsService.UpdateOwnerAsync(id, ownerId);
            if (!updated)
                return NotFound();

            return Ok(new { CollectionId = id, OwnerId = ownerId });
        }
    }
}
