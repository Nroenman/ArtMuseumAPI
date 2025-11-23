using Microsoft.EntityFrameworkCore;
using MysqlMongodbMigrator.Models;

namespace MysqlMongodbMigrator.Data
{
    public class KunstMuseumDbContext : DbContext
    {
        public KunstMuseumDbContext(DbContextOptions<KunstMuseumDbContext> options)
            : base(options)
        {}

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

        modelBuilder.Entity<Artists>(entity =>
        {
            entity.ToTable("artists");
            entity.Property(e => e.ArtistID).HasColumnName("artist_id");
            entity.Property(e => e.FullName).HasColumnName("full_name");
            entity.Property(e => e.Nationality).HasColumnName("nationality");
            entity.Property(e => e.BirthDate).HasColumnName("birth_date");
            entity.Property(e => e.DeathDate).HasColumnName("death_date");
            entity.Property(e => e.Biography).HasColumnName("biography");
        });

        modelBuilder.Entity<Artworks>(entity =>
        {
            entity.ToTable("artworks");
            entity.Property(e => e.ArtworkID).HasColumnName("artwork_id");
            entity.Property(e => e.Title).HasColumnName("title");
            entity.Property(e => e.Medium).HasColumnName("medium");
            entity.Property(e => e.YearCreated).HasColumnName("year_created");
            entity.Property(e => e.Dimensions).HasColumnName("dimensions");
            entity.Property(e => e.PrimaryArtistID).HasColumnName("primary_artist_id");
            entity.Property(e => e.CurrentLocationID).HasColumnName("current_location_id");
            entity.Property(e => e.CurrentOwnerID).HasColumnName("current_owner_id");
            entity.Property(e => e.Notes).HasColumnName("notes");

            entity.HasOne(e => e.PrimaryArtist)
                .WithMany()
                .HasForeignKey(e => e.PrimaryArtistID);

            entity.HasOne(e => e.CurrentLocation)
                .WithMany()
                .HasForeignKey(e => e.CurrentLocationID);

            entity.HasOne(e => e.CurrentOwner)
                .WithMany()
                .HasForeignKey(e => e.CurrentOwnerID);
        });

        modelBuilder.Entity<CollectionItems>(entity =>
        {
            entity.ToTable("collection_items");
            entity.Property(e => e.CollectionID).HasColumnName("collection_id");
            entity.Property(e => e.ArtworkID).HasColumnName("artwork_id");
            entity.Property(e => e.DateAdded).HasColumnName("date_added");
            entity.Property(e => e.ItemNotes).HasColumnName("item_notes");

            entity.HasOne(e => e.Collection)
            .WithMany(c => c.CollectionItems)
            .HasForeignKey(e => e.CollectionID);
        });

        modelBuilder.Entity<Collections>(entity =>
        {
            entity.ToTable("collections");
            entity.Property(e => e.CollectionID).HasColumnName("collection_id");
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.OwnerID).HasColumnName("owner_id");
        });

        modelBuilder.Entity<ExhibitionArtworks>(entity =>
        {
            entity.ToTable("exhibition_artworks");
            entity.Property(e => e.ExhibitionID).HasColumnName("exhibition_id");
            entity.Property(e => e.ArtworkID).HasColumnName("artwork_id");
            entity.Property(e => e.DisplayLabel).HasColumnName("display_label");
            entity.Property(e => e.Notes).HasColumnName("notes");

            entity.HasOne(e => e.Exhibition)
                .WithMany(ex => ex.ExhibitionArtworks)
                .HasForeignKey(e => e.ExhibitionID);

            entity.HasOne(e => e.Artwork)
                .WithMany()
                .HasForeignKey(e => e.ArtworkID);
        });

        modelBuilder.Entity<Exhibitions>(entity =>
        {
            entity.ToTable("exhibitions");
            entity.Property(e => e.ExhibitionID).HasColumnName("exhibition_id");
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.StartDate).HasColumnName("start_date");
            entity.Property(e => e.EndDate).HasColumnName("end_date");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.LocationID).HasColumnName("location_id");
        });

        modelBuilder.Entity<Locations>(entity =>
        {
            entity.ToTable("locations");
            entity.Property(e => e.LocationID).HasColumnName("location_id");
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.Address).HasColumnName("address");
            entity.Property(e => e.Room).HasColumnName("room");
            entity.Property(e => e.Shelf).HasColumnName("shelf");
        });

        modelBuilder.Entity<MediaFiles>(entity =>
        {
            entity.ToTable("media_files");
            entity.Property(e => e.MediaFileID).HasColumnName("media_id");
            entity.Property(e => e.ArtworkID).HasColumnName("artwork_id");
            entity.Property(e => e.ArtistID).HasColumnName("artist_id");
            entity.Property(e => e.MediaType).HasColumnName("media_type");
            entity.Property(e => e.Title).HasColumnName("title");
            entity.Property(e => e.FileURL).HasColumnName("file_url");
            entity.Property(e => e.CapturedDate).HasColumnName("captured_date");
            entity.Property(e => e.CopyrightHolder).HasColumnName("copyright_holder");
            entity.Property(e => e.Notes).HasColumnName("notes");

            entity.HasOne(e => e.Artwork)
                .WithMany()
                .HasForeignKey(e => e.ArtworkID);
        });

        modelBuilder.Entity<Owners>(entity =>
        {
            entity.ToTable("owners");
            entity.Property(e => e.OwnerID).HasColumnName("owner_id");
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.OwnerType).HasColumnName("owner_type");
            entity.Property(e => e.ContactEmail).HasColumnName("contact_email");
            entity.Property(e => e.Phone).HasColumnName("phone");
            entity.Property(e => e.Address).HasColumnName("address");
        });

        modelBuilder.Entity<Ownership>(entity =>
        {
            entity.ToTable("ownership");
            entity.Property(e => e.OwnershipID).HasColumnName("ownership_id");
            entity.Property(e => e.ArtworkID).HasColumnName("artwork_id");
            entity.Property(e => e.OwnerID).HasColumnName("owner_id");
            entity.Property(e => e.AcquiredDate).HasColumnName("acquired_date");
            entity.Property(e => e.RelinquishedDate).HasColumnName("relinquished_date");
            entity.Property(e => e.SourceDocument).HasColumnName("source_document");
            entity.Property(e => e.Notes).HasColumnName("notes");

            entity.HasOne(e => e.Artwork)
                .WithMany()
                .HasForeignKey(e => e.ArtworkID);

            entity.HasOne(e => e.Owner)
                .WithMany(o => o.Ownerships)
                .HasForeignKey(e => e.OwnerID)
                .HasPrincipalKey(o => o.OwnerID); 
        });

        modelBuilder.Entity<Restorations>(entity =>
        {
            entity.ToTable("restorations");
            entity.Property(e => e.RestorationID).HasColumnName("restoration_id");
            entity.Property(e => e.ArtworkID).HasColumnName("artwork_id");
            entity.Property(e => e.RestorationDate).HasColumnName("restoration_date");
            entity.Property(e => e.Conservator).HasColumnName("conservator");
            entity.Property(e => e.RestorationType).HasColumnName("restoration_type");
            entity.Property(e => e.Details).HasColumnName("details");
            entity.Property(e => e.ConditionBefore).HasColumnName("condition_before");
            entity.Property(e => e.ConditionAfter).HasColumnName("condition_after");
            entity.Property(e => e.Cost).HasColumnName("cost");
            entity.Property(e => e.Currency).HasColumnName("currency");
        });

        modelBuilder.Entity<Transactions>(entity =>
        {
            entity.ToTable("transactions");
            entity.Property(e => e.TransactionID).HasColumnName("transaction_id");
            entity.Property(e => e.ArtworkID).HasColumnName("artwork_id");
            entity.Property(e => e.TxnDate).HasColumnName("txn_date");
            entity.Property(e => e.TxnType).HasColumnName("txn_type");
            entity.Property(e => e.FromOwnerID).HasColumnName("from_owner_id");
            entity.Property(e => e.ToOwnerID).HasColumnName("to_owner_id");
            entity.Property(e => e.Price).HasColumnName("price");
            entity.Property(e => e.Currency).HasColumnName("currency");
            entity.Property(e => e.Notes).HasColumnName("notes");

            entity.HasOne(e => e.Artwork)
                .WithMany()
                .HasForeignKey(e => e.ArtworkID);

            entity.HasOne(e => e.FromOwner)
                .WithMany(o => o.TransactionsFrom)
                .HasForeignKey(e => e.FromOwnerID);

            entity.HasOne(e => e.ToOwner)
                .WithMany(o => o.TransactionsTo)
                .HasForeignKey(e => e.ToOwnerID);
        });

        modelBuilder.Entity<Users>(entity =>
        {
            entity.ToTable("users");
            entity.Property(e => e.UserID).HasColumnName("UserId");
            entity.Property(e => e.UserName).HasColumnName("UserName");
            entity.Property(e => e.Email).HasColumnName("Email");
            entity.Property(e => e.PasswordHash).HasColumnName("PasswordHash");
            entity.Property(e => e.CreatedAt).HasColumnName("CreatedAt");
            entity.Property(e => e.UpdatedAt).HasColumnName("UpdatedAt");
            entity.Property(e => e.Roles).HasColumnName("Roles");
        });
        }

    public DbSet<Artists> Artists { get; set; }
    public DbSet<Artworks> Artworks { get; set; }
    public DbSet<CollectionItems> CollectionItems { get; set; }
    public DbSet<Collections> Collections { get; set; }
    public DbSet<ExhibitionArtworks> ExhibitionArtworks { get; set; }
    public DbSet<Exhibitions> Exhibitions { get; set; }
    public DbSet<Locations> Locations { get; set; }
    public DbSet<MediaFiles> MediaFiles { get; set; }
    public DbSet<Owners> Owners { get; set; }
    public DbSet<Ownership> Ownerships { get; set; }
    public DbSet<Restorations> Restorations { get; set; }
    public DbSet<Transactions> Transactions { get; set; }
    public DbSet<Users> Users { get; set; }
    }
}