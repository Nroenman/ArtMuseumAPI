using ArtMuseumAPI.Documents;

namespace ArtMuseumAPI.Services;

public interface IArtistsService
{
    Task<List<ArtistsDocument>> GetAllAsync();
    Task<ArtistsDocument?> GetByIdAsync(string id);
    Task CreateAsync(ArtistsDocument artist);
    Task UpdateAsync(string id, ArtistsDocument artist);
    Task DeleteAsync(string id);
}