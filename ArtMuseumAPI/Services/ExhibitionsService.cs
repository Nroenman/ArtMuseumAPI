using ArtMuseumAPI.Documents;
using MongoDB.Driver;

namespace ArtMuseumAPI.Services;

public class ExhibitionsService : IExhibitionsService
{
    private readonly IMongoCollection<ExhibitionsDocument> _collection;

    public ExhibitionsService(IMongoService mongoService)
    {
        _collection = mongoService.GetCollection<ExhibitionsDocument>("exhibitions");
    }

    public async Task<List<ExhibitionsDocument>> GetAllAsync()
    {
        return await _collection.Find(_ => true).ToListAsync();
    }

    public async Task<ExhibitionsDocument?> GetByIdAsync(string id)
    {
        return await _collection.Find(x => x.Id == id).FirstOrDefaultAsync();
    }

    public async Task CreateAsync(ExhibitionsDocument exhibition)
    {
        await _collection.InsertOneAsync(exhibition);
    }

    public async Task UpdateAsync(string id, ExhibitionsDocument exhibition)
    {
        await _collection.ReplaceOneAsync(x => x.Id == id, exhibition);
    }

    public async Task DeleteAsync(string id)
    {
        await _collection.DeleteOneAsync(x => x.Id == id);
    }
}