using ArtMuseumAPI.Documents;
using MongoDB.Driver;

namespace ArtMuseumAPI.Services;

public class ArtistsService : IArtistsService
{
    private readonly IMongoCollection<ArtistsDocument> _collection;

    public ArtistsService(IMongoService mongoService)
    {
        _collection = mongoService.GetCollection<ArtistsDocument>("artists");
    }

    public async Task<List<ArtistsDocument>> GetAllAsync()
    {
        return await _collection.Find(_ => true).ToListAsync();
    }

    public async Task<ArtistsDocument?> GetByIdAsync(string id)
    {
        return await _collection.Find(x => x.Id == id).FirstOrDefaultAsync();
    }

    public async Task CreateAsync(ArtistsDocument artist)
    {
        await _collection.InsertOneAsync(artist);
    }

    public async Task UpdateAsync(string id, ArtistsDocument artist)
    {
        await _collection.ReplaceOneAsync(x => x.Id == id, artist);
    }

    public async Task DeleteAsync(string id)
    {
        await _collection.DeleteOneAsync(x => x.Id == id);
    }
}