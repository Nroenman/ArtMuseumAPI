using ArtMuseumAPI.DTO;
using ArtMuseumAPI.Models;

namespace ArtMuseumAPI.Services
{
    public interface ICollectionsService
    {
        Task<Collection?> GetCollectionAsync(int id);
        Task<Collection> CreateCollectionAsync(AddCollectionRequest request);
        Task<bool> UpdateOwnerAsync(int collectionId, int ownerId);
        Task<bool> DeleteCollectionAsync(int id);
    }
}
