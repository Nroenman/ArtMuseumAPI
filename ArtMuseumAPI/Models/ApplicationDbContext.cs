using Microsoft.EntityFrameworkCore;

namespace ArtMuseumAPI.Models;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(e =>
        {
            e.ToTable("users");
            e.HasKey(u => u.UserId);
            e.Property(u => u.UserId).ValueGeneratedOnAdd(); // important for auto-increment
            e.Property(u => u.UserName).HasMaxLength(100).IsRequired();
            e.Property(u => u.Email).HasMaxLength(100).IsRequired();
            e.Property(u => u.PasswordHash).HasMaxLength(100).IsRequired();
            e.Property(u => u.Roles).HasMaxLength(50).IsRequired();
            e.Property(u => u.CreatedAt).IsRequired();
            e.Property(u => u.UpdatedAt).IsRequired();
        });
    }

}