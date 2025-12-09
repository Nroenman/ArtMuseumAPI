using ArtMuseumAPI.DTO;
using ArtMuseumAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace ArtMuseumAPI.Services;

public class CollectionsService(ApplicationDbContext db) : ICollectionsService
{
    private readonly ApplicationDbContext _db = db;

    public async Task<Collection?> GetCollectionAsync(int id)
    {
        return await _db.Collections
            .FirstOrDefaultAsync(c => c.CollectionId == id);
    }

    public async Task<Collection> CreateCollectionAsync(AddCollectionRequest request)
    {
        var collection = new Collection
        {
            Name = request.Name,
            Description = request.Description,
            OwnerId = request.Owner
        };

        _db.Collections.Add(collection);
        await _db.SaveChangesAsync();

        return collection;
    }

    public async Task<bool> UpdateOwnerAsync(int id, int ownerId)
    {
        var collection = await _db.Collections
            .FirstOrDefaultAsync(c => c.CollectionId == id);

        if (collection == null)
            return false;

        collection.OwnerId = ownerId;
        await _db.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteCollectionAsync(int id)
    {
        var exists = await _db.Collections
            .AnyAsync(c => c.CollectionId == id);

        if (!exists)
            return false;

        // Keep your stored procedure
        await _db.Database.ExecuteSqlRawAsync("CALL delete_collection({0})", id);

        return true;
    }
}
