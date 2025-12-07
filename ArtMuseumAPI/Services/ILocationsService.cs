using ArtMuseumAPI.Documents;

namespace ArtMuseumAPI.Services;

public interface ILocationsService
{
    Task<List<LocationsDocument>> GetAllAsync();
    Task<LocationsDocument?> GetByIdAsync(string id);
    Task CreateAsync(LocationsDocument location);
    Task UpdateAsync(string id, LocationsDocument location);
    Task DeleteAsync(string id);
}