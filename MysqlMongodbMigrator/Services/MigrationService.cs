using Microsoft.EntityFrameworkCore;
using MysqlMongodbMigrator.Models.Mongo;
using MysqlMongodbMigrator.Data;


namespace MysqlMongodbMigrator.Services
{
    public class MigrationService
    {
        private readonly KunstMuseumDbContext _dbContext;
        private readonly MongoDbService _mongo;

        public MigrationService(KunstMuseumDbContext dbContext, MongoDbService mongo)
        {
            _dbContext = dbContext;
            _mongo = mongo;
        }

        public async Task MigrateAsync()
        {
            Console.WriteLine("Starting migration");

            await MigrateArtistsAsync();
            await MigrateArtworksAsync();
            await MigrateCollectionItemsAsync();
            await MigrateCollectionsAsync();
            await MigrateExhibitionArtworksAsync();
            await MigrateExhibitionsAsync();
            await MigrateLocationsAsync();
            await MigrateMediaFilesAsync();
            await MigrateOwnersAsync();
            await MigrateOwnershipsAsync();
            await MigrateRestorationsAsync();
            await MigrateTransactionsAsync();
            await MigrateUsersAsync();
            
            Console.WriteLine("Migration completed");
        }

        private async Task MigrateArtistsAsync()
        {
            var artists = await _dbContext.Artists.ToListAsync();

            var artistsDocs = new List<ArtistsDocument>();
            foreach (var artist in artists)
            {
                artistsDocs.Add(new ArtistsDocument
                {
                    ArtistID = artist.ArtistID,
                    FullName = artist.FullName,
                    Nationality = artist.Nationality,
                    BirthDate = artist.BirthDate,
                    DeathDate = artist.DeathDate,
                    Biography = artist.Biography
                });
            }

            await _mongo.Artists.InsertManyAsync(artistsDocs);
        }

        private async Task MigrateArtworksAsync()
        {
            var artworks = await _dbContext.Artworks.ToListAsync();

            var artworksDocs = new List<ArtworksDocument>();
            foreach (var artwork in artworks)
            {
                artworksDocs.Add(new ArtworksDocument
                {
                    ArtworkID = artwork.ArtworkID,
                    Title = artwork.Title,
                    Medium = artwork.Medium,
                    YearCreated = artwork.YearCreated,
                    Dimensions = artwork.Dimensions,
                    PrimaryArtistID = artwork.PrimaryArtistID,
                    CurrentLocationID = artwork.CurrentLocationID,
                    CurrentOwnerID = artwork.CurrentOwnerID,
                    Notes = artwork.Notes,
                });
            }

            await _mongo.Artworks.InsertManyAsync(artworksDocs);
        }

        private async Task MigrateCollectionItemsAsync()
        {
            var collectionItems = await _dbContext.CollectionItems.ToListAsync();

            var collectionItemsDocs = new List<CollectionItemsDocument>();
            foreach (var item in collectionItems)
            {
                collectionItemsDocs.Add(new CollectionItemsDocument
                {
                    CollectionID = item.CollectionID,
                    ArtworkID = item.ArtworkID,
                    DateAdded = item.DateAdded,
                    ItemNotes = item.ItemNotes
                });
            }

            await _mongo.CollectionItems.InsertManyAsync(collectionItemsDocs);
        }

        private async Task MigrateCollectionsAsync()
        {
            var collections = await _dbContext.Collections.ToListAsync();

            var collectionsDocs = new List<CollectionsDocument>();
            foreach (var collection in collections)
            {
                collectionsDocs.Add(new CollectionsDocument
                {
                    CollectionID = collection.CollectionID,
                    Name = collection.Name,
                    Description = collection.Description,
                    OwnerID = collection.OwnerID
                });
            }

            await _mongo.Collections.InsertManyAsync(collectionsDocs);
        }

        private async Task MigrateExhibitionArtworksAsync()
        {
            var exhibitionArtworks = await _dbContext.ExhibitionArtworks.ToListAsync();

            var exhibitionArtworksDocs = new List<ExhibitionArtworksDocument>();
            foreach (var item in exhibitionArtworks)
            {
                exhibitionArtworksDocs.Add(new ExhibitionArtworksDocument
                {
                    ExhibitionID = item.ExhibitionID,
                    ArtworkID = item.ArtworkID,
                    DisplayLabel = item.DisplayLabel,
                    Notes = item.Notes
                });
            }

            await _mongo.ExhibitionArtworks.InsertManyAsync(exhibitionArtworksDocs);
        }

        private async Task MigrateExhibitionsAsync()
        {
            var exhibitions = await _dbContext.Exhibitions.ToListAsync();

            var exhibitionsDocs = new List<ExhibitionsDocument>();
            foreach (var exhibition in exhibitions)
            {
                exhibitionsDocs.Add(new ExhibitionsDocument
                {
                    ExhibitionID = exhibition.ExhibitionID,
                    Name = exhibition.Name,
                    StartDate = exhibition.StartDate,
                    EndDate = exhibition.EndDate,
                    Description = exhibition.Description,
                    LocationID = exhibition.LocationID
                });
            }

            await _mongo.Exhibitions.InsertManyAsync(exhibitionsDocs);
        }

        private async Task MigrateLocationsAsync()
        {
            var locations = await _dbContext.Locations.ToListAsync();

            var locationsDocs = new List<LocationsDocument>();
            foreach (var location in locations)
            {
                locationsDocs.Add(new LocationsDocument
                {
                    LocationID = location.LocationID,
                    Name = location.Name,
                    Address = location.Address,
                    Room = location.Room,
                    Shelf = location.Shelf
                });
            }

            await _mongo.Locations.InsertManyAsync(locationsDocs);
        }

        private async Task MigrateMediaFilesAsync()
        {
            var mediaFiles = await _dbContext.MediaFiles.ToListAsync();

            var mediaFilesDocs = new List<MediaFilesDocument>();
            foreach (var mediaFile in mediaFiles)
            {
                mediaFilesDocs.Add(new MediaFilesDocument
                {
                    MediaFileID = mediaFile.MediaFileID,
                    ArtworkID = mediaFile.ArtworkID,
                    Artwork = mediaFile.Artwork,
                    ArtistID = mediaFile.ArtistID,
                    Artist = mediaFile.Artist,
                    MediaType = mediaFile.MediaType,
                    Title = mediaFile.Title,
                    FileURL = mediaFile.FileURL,
                    CapturedDate = mediaFile.CapturedDate,
                    CopyrightHolder = mediaFile.CopyrightHolder,
                    Notes = mediaFile.Notes
                });
            }

            await _mongo.MediaFiles.InsertManyAsync(mediaFilesDocs);
        }

        private async Task MigrateOwnersAsync()
        {
            var owners = await _dbContext.Owners.ToListAsync();

            var ownersDocs = new List<OwnersDocument>();
            foreach (var owner in owners)
            {
                ownersDocs.Add(new OwnersDocument
                {
                    OwnerID = owner.OwnerID,
                    Name = owner.Name,
                    OwnerType = owner.OwnerType,
                    ContactEmail = owner.ContactEmail,
                    Phone = owner.Phone,
                    Address = owner.Address
                });
            }

            await _mongo.Owners.InsertManyAsync(ownersDocs);
        }

        private async Task MigrateOwnershipsAsync()
        {
            var ownerships = await _dbContext.Ownerships.ToListAsync();

            var ownershipsDocs = new List<OwnershipDocument>();
            foreach (var ownership in ownerships)
            {
                ownershipsDocs.Add(new OwnershipDocument
                {
                    OwnershipID = ownership.OwnershipID,
                    ArtworkID = ownership.ArtworkID,
                    OwnerID = ownership.OwnerID,
                    AcquiredDate = ownership.AcquiredDate,
                    RelinquishedDate = ownership.RelinquishedDate,
                    SourceDocument = ownership.SourceDocument,
                    Notes = ownership.Notes
                });
            }

            await _mongo.Ownerships.InsertManyAsync(ownershipsDocs);
        }

        private async Task MigrateRestorationsAsync()
        {
            var restorations = await _dbContext.Restorations.ToListAsync();

            var restorationsDocs = new List<RestorationsDocument>();
            foreach (var restoration in restorations)
            {
                restorationsDocs.Add(new RestorationsDocument
                {
                    RestorationID = restoration.RestorationID,
                    ArtworkID = restoration.ArtworkID,
                    RestorationDate = restoration.RestorationDate,
                    Conservator = restoration.Conservator,
                    RestorationType = restoration.RestorationType,
                    Details = restoration.Details,
                    ConditionBefore = restoration.ConditionBefore,
                    ConditionAfter = restoration.ConditionAfter,
                    Cost = restoration.Cost,
                    Currency = restoration.Currency
                });
            }

            await _mongo.Restorations.InsertManyAsync(restorationsDocs);
        }

        private async Task MigrateTransactionsAsync()
        {
            var transactions = await _dbContext.Transactions.ToListAsync();

            var transactionsDocs = new List<TransactionsDocument>();
            foreach (var transaction in transactions)
            {
                transactionsDocs.Add(new TransactionsDocument
                {
                    TransactionID = transaction.TransactionID,
                    ArtworkID = transaction.ArtworkID,
                    TxnDate = transaction.TxnDate,
                    TxnType = transaction.TxnType,
                    FromOwnerID = transaction.FromOwnerID,
                    ToOwnerID = transaction.ToOwnerID,
                    Price = transaction.Price,
                    Currency = transaction.Currency,
                    Notes = transaction.Notes
                });
            }

            await _mongo.Transactions.InsertManyAsync(transactionsDocs);
        }

        private async Task MigrateUsersAsync()
        {
            var users = await _dbContext.Users.ToListAsync();

            var usersDocs = new List<UsersDocument>();
            foreach (var user in users)
            {
                usersDocs.Add(new UsersDocument
                {
                    UserID = user.UserID,
                    UserName = user.UserName,
                    Email = user.Email,
                    PasswordHash = user.PasswordHash,
                    CreatedAt = user.CreatedAt,
                    UpdatedAt = user.UpdatedAt,
                    Roles = user.Roles,
                });
            }

            await _mongo.Users.InsertManyAsync(usersDocs);
        }
    }
}