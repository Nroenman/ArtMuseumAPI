using ArtMuseumAPI.Models;

namespace ArtMuseumAPI.Services;

public interface IUserService
{
    User? Authenticate(string email, string password);  
    User GetUserFromJwtToken(string token);             
}