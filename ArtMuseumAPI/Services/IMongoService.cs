using MongoDB.Driver;

namespace ArtMuseumAPI.Services;

public interface IMongoService
{
    IMongoCollection<T> GetCollection<T>(string collectionName);
}