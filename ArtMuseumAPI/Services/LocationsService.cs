using ArtMuseumAPI.Documents;
using MongoDB.Driver;

namespace ArtMuseumAPI.Services;

public class LocationsService : ILocationsService
{
    private readonly IMongoCollection<LocationsDocument> _collection;

    public LocationsService(IMongoService mongoService)
    {
        _collection = mongoService.GetCollection<LocationsDocument>("locations");
    }

    public async Task<List<LocationsDocument>> GetAllAsync()
    {
        return await _collection.Find(_ => true).ToListAsync();
    }

    public async Task<LocationsDocument?> GetByIdAsync(string id)
    {
        return await _collection.Find(x => x.Id == id).FirstOrDefaultAsync();
    }

    public async Task CreateAsync(LocationsDocument location)
    {
        await _collection.InsertOneAsync(location);
    }

    public async Task UpdateAsync(string id, LocationsDocument location)
    {
        await _collection.ReplaceOneAsync(x => x.Id == id, location);
    }

    public async Task DeleteAsync(string id)
    {
        await _collection.DeleteOneAsync(x => x.Id == id);
    }
}