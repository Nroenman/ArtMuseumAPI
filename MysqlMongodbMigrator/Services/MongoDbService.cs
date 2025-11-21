using MongoDB.Driver;
using MysqlMongodbMigrator.Models.Mongo;


namespace MysqlMongodbMigrator.Services
{
    public class MongoDbService
    {
        private readonly IMongoDatabase _database;

        public MongoDbService(string connectionString, string databaseName)
        {
            var client = new MongoClient(connectionString);
            _database = client.GetDatabase(databaseName);
        }

        public IMongoCollection<ArtistsDocument> Artists =>
            _database.GetCollection<ArtistsDocument>("Artists");

        public IMongoCollection<ArtworksDocument> Artworks =>
            _database.GetCollection<ArtworksDocument>("Artworks");

        public IMongoCollection<CollectionItemsDocument> CollectionItems =>
            _database.GetCollection<CollectionItemsDocument>("CollectionItems");

        public IMongoCollection<CollectionsDocument> Collections =>
            _database.GetCollection<CollectionsDocument>("Collections");

        public IMongoCollection<ExhibitionArtworksDocument> ExhibitionArtworks =>
            _database.GetCollection<ExhibitionArtworksDocument>("ExhibitionArtworks");

        public IMongoCollection<ExhibitionsDocument> Exhibitions =>
            _database.GetCollection<ExhibitionsDocument>("Exhibitions");

        public IMongoCollection<LocationsDocument> Locations =>
            _database.GetCollection<LocationsDocument>("Locations");

        public IMongoCollection<MediaFilesDocument> MediaFiles =>
            _database.GetCollection<MediaFilesDocument>("MediaFiles");

        public IMongoCollection<OwnersDocument> Owners =>
            _database.GetCollection<OwnersDocument>("Owners");

        public IMongoCollection<OwnershipDocument> Ownerships =>
            _database.GetCollection<OwnershipDocument>("Ownerships");

        public IMongoCollection<RestorationsDocument> Restorations =>
            _database.GetCollection<RestorationsDocument>("Restorations");

        public IMongoCollection<TransactionsDocument> Transactions =>
            _database.GetCollection<TransactionsDocument>("Transactions");

        public IMongoCollection<UsersDocument> Users =>
            _database.GetCollection<UsersDocument>("Users");
    }
}