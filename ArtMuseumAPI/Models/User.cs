using System.Text.Json.Serialization;

namespace ArtMuseumAPI.Models;

public class User
{
    // EF can set this when inserting a row
    public int UserId { get; set; }
    public string UserName { get; set; } = null!;
    public string Email { get; set; } = null!;

    [JsonIgnore] // avoid leaking hashes in API responses/logs
    public string PasswordHash { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public string Roles { get; set; } = "user";

    public User() { }

    public User(int userId, string userName, string email, string passwordHash, DateTime createdAt, DateTime updatedAt, string roles)
    {
        UserId = userId;
        UserName = userName;
        Email = email;
        PasswordHash = passwordHash;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
        Roles = roles;
    }

    public override string ToString()
        => $"User ID: {UserId}, Name: {UserName}, Email: {Email}, Created At: {CreatedAt}, Updated At: {UpdatedAt}, Roles: {Roles}";
}