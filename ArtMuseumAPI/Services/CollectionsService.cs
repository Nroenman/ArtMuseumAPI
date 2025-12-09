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
        await using var tx = await _db.Database.BeginTransactionAsync();

        try
        {
            var exists = await _db.Collections
                .AnyAsync(c => c.CollectionId == id);

            if (!exists)
                return false; 
            
            await _db.Database.ExecuteSqlRawAsync("CALL delete_collection({0})", id);

            await tx.CommitAsync();
            return true;
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }
}
