using ArtMuseumAPI.Documents;

namespace ArtMuseumAPI.Services;

public interface IExhibitionsService
{
    Task<List<ExhibitionsDocument>> GetAllAsync();
    Task<ExhibitionsDocument?> GetByIdAsync(string id);
    Task CreateAsync(ExhibitionsDocument exhibition);
    Task UpdateAsync(string id, ExhibitionsDocument exhibition);
    Task DeleteAsync(string id);
}