using ArtMuseumAPI.Documents;

namespace ArtMuseumAPI.Services;

public interface IArtworksService
{
    Task<List<ArtworksDocument>> GetAllAsync();
    Task<ArtworksDocument?> GetByIdAsync(string id);
    Task CreateAsync(ArtworksDocument artwork);
    Task UpdateAsync(string id, ArtworksDocument artwork);
    Task DeleteAsync(string id);
}