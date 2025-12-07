using ArtMuseumAPI.Documents;
using MongoDB.Driver;

namespace ArtMuseumAPI.Services;

public class ArtworksService : IArtworksService
{
    private readonly IMongoCollection<ArtworksDocument> _collection;

    public ArtworksService(IMongoDatabase database)
    {
        _collection = database.GetCollection<ArtworksDocument>("artworks");
    }

    public async Task<List<ArtworksDocument>> GetAllAsync()
    {
        return await _collection.Find(_ => true).ToListAsync();
    }

    public async Task<ArtworksDocument?> GetByIdAsync(string id)
    {
        return await _collection.Find(doc => doc.Id == id).FirstOrDefaultAsync();
    }

    public async Task CreateAsync(ArtworksDocument artwork)
    {
        await _collection.InsertOneAsync(artwork);
    }

    public async Task UpdateAsync(string id, ArtworksDocument updatedArtwork)
    {
        await _collection.ReplaceOneAsync(doc => doc.Id == id, updatedArtwork);
    }

    public async Task DeleteAsync(string id)
    {
        await _collection.DeleteOneAsync(doc => doc.Id == id);
    }
}