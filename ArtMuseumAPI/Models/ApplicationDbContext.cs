using Microsoft.EntityFrameworkCore;

namespace ArtMuseumAPI.Models;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
  

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Collection> Collections { get; set; } = null!;

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
        modelBuilder.Entity<Collection>(e =>
        {
            e.ToTable("collections");
            e.HasKey(c => c.CollectionId);
            e.Property(c => c.CollectionId).HasColumnName("collection_id").ValueGeneratedOnAdd();
            e.Property(c => c.Name).HasColumnName("name").HasMaxLength(255).IsRequired();
            e.Property(c => c.Description).HasColumnName("description");
            e.Property(c => c.OwnerId).HasColumnName("owner_id");
            e.HasIndex(c => c.Name).HasDatabaseName("idx_collections_name");
            e.HasIndex(c => c.OwnerId).HasDatabaseName("idx_collections_owner");
        });

    }

}