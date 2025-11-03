namespace ArtMuseumAPI.Models;

public class User
{
    public int UserId { get; set; }
    public string UserName { get; set; }
    public string Email { get; set; }
    public string PasswordHash {get; set;}
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string Roles { get; set; }




    public User()
    {
        
    }
   



    public User(string userName, string email, string passwordHash, DateTime createdAt, DateTime updatedAt,
        string roles)
    {
        UserId = UserId;
        UserName = userName;
        Email = email;
        PasswordHash = passwordHash;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
        Roles = roles;
        
    }

    public override string ToString()
    {
        return $"User ID: {UserId}, Name: {UserName}, Email: {Email}, Password: {PasswordHash}, Created At: {CreatedAt}, Updated At: {UpdatedAt}, Roles: {Roles}";
    }
}