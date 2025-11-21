using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using Neo4j.Driver;

namespace ArtMuseumMigrationneo4js;

internal class Program
{
    static async Task Main()
    {
        try
        {
            Console.WriteLine("Starting migration...");

            // --- Load configuration + secrets ---
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true)
                .AddUserSecrets<Program>() // keep passwords out of git
                .Build();

            string mysqlConn = config.GetConnectionString("DefaultConnection")
                               ?? throw new Exception("Missing MySQL connection string");

            Console.WriteLine("MySQL conn string (debug, masked):");
            Console.WriteLine(mysqlConn.Replace(
                mysqlConn.Contains("password=", StringComparison.OrdinalIgnoreCase)
                    ? mysqlConn.Substring(mysqlConn.IndexOf("password=", StringComparison.OrdinalIgnoreCase) + 9)
                    : "",
                "*****"
            ));

            string neo4JUri  = config["Neo4j:Uri"]      ?? throw new Exception("Missing Neo4j:Uri");
            string neo4JUser = config["Neo4j:User"]     ?? "neo4j";
            string neo4JPass = config["Neo4j:Password"] ?? throw new Exception("Missing Neo4j:Password");

            Console.WriteLine($"Neo4j URI (debug): {neo4JUri}");

            // --- Test MySQL connection once ---
            await using (var testConn = new MySqlConnection(mysqlConn))
            {
                await testConn.OpenAsync();
                Console.WriteLine("MySQL connected.");
            }

            // --- Neo4j connection ---
            await using var driver = GraphDatabase.Driver(
                neo4JUri,
                AuthTokens.Basic(neo4JUser, neo4JPass)
            );

            Console.WriteLine("Testing Neo4j connectivity...");
            await driver.VerifyConnectivityAsync();
            Console.WriteLine("Neo4j connectivity OK.");

            await using var session = driver.AsyncSession(o => o.WithDatabase("neo4j"));
            Console.WriteLine("Neo4j session opened.");

            // 1) Constraints (run once, safe to re-run)
            await CreateConstraints(session);

            // 2) Migrate NODE tables first (each gets its own MySQL connection)
            await MigrateArtists(mysqlConn, session);
            await MigrateLocations(mysqlConn, session);
            await MigrateOwners(mysqlConn, session);
            await MigrateExhibitions(mysqlConn, session);
            await MigrateCollections(mysqlConn, session);
            await MigrateArtworks(mysqlConn, session);
            await MigrateMedia(mysqlConn, session);
            await MigrateUsers(mysqlConn, session);
            await MigrateRestorationNodes(mysqlConn, session);
            await MigrateTransactionNodes(mysqlConn, session);

            // 3) Then migrate RELATIONSHIPS (FKs + join tables)
            await LinkArtworksToArtists(mysqlConn, session);
            await LinkArtworksToLocations(mysqlConn, session);
            await LinkArtworksToOwners(mysqlConn, session);
            await MigrateCollectionItemsRels(mysqlConn, session);
            await MigrateExhibitionArtworksRels(mysqlConn, session);
            await MigrateMediaRelations(mysqlConn, session);
            await MigrateOwnershipRels(mysqlConn, session);
            await LinkRestorations(mysqlConn, session);
            await LinkTransactions(mysqlConn, session);

            Console.WriteLine("Migration complete!");
        }
        catch (Exception ex)
        {
            Console.WriteLine("FATAL ERROR DURING MIGRATION:");
            Console.WriteLine(ex);
        }
    }

    // =========================
    // Constraints
    // =========================
    private static async Task CreateConstraints(IAsyncSession session)
    {
        var commands = new[]
        {
            "CREATE CONSTRAINT artist_id_unique IF NOT EXISTS FOR (a:Artist)       REQUIRE a.artistId       IS UNIQUE",
            "CREATE CONSTRAINT location_id_unique IF NOT EXISTS FOR (l:Location)   REQUIRE l.locationId     IS UNIQUE",
            "CREATE CONSTRAINT owner_id_unique IF NOT EXISTS FOR (o:Owner)         REQUIRE o.ownerId        IS UNIQUE",
            "CREATE CONSTRAINT exhibition_id_unique IF NOT EXISTS FOR (e:Exhibition) REQUIRE e.exhibitionId IS UNIQUE",
            "CREATE CONSTRAINT collection_id_unique IF NOT EXISTS FOR (c:Collection) REQUIRE c.collectionId IS UNIQUE",
            "CREATE CONSTRAINT artwork_id_unique IF NOT EXISTS FOR (w:Artwork)     REQUIRE w.artworkId      IS UNIQUE",
            "CREATE CONSTRAINT media_id_unique IF NOT EXISTS FOR (m:Media)         REQUIRE m.mediaId        IS UNIQUE",
            "CREATE CONSTRAINT appuser_id_unique IF NOT EXISTS FOR (u:AppUser)     REQUIRE u.userId         IS UNIQUE",
            "CREATE CONSTRAINT restoration_id_unique IF NOT EXISTS FOR (r:Restoration) REQUIRE r.restorationId IS UNIQUE",
            "CREATE CONSTRAINT transaction_id_unique IF NOT EXISTS FOR (t:Transaction) REQUIRE t.transactionId IS UNIQUE"
        };

        foreach (var cmd in commands)
        {
            await session.RunAsync(cmd);
        }

        Console.WriteLine("Constraints created.");
    }

    // =========================
    // Node migrations
    // =========================

    private static async Task MigrateArtists(string connStr, IAsyncSession session)
    {
        const string sql = @"SELECT artist_id, full_name, nationality, birth_date, death_date, biography FROM artists";

        await using var mysql = new MySqlConnection(connStr);
        await mysql.OpenAsync();

        await using var cmd = new MySqlCommand(sql, mysql);
        await using var reader = await cmd.ExecuteReaderAsync();

        int colId   = reader.GetOrdinal("artist_id");
        int colName = reader.GetOrdinal("full_name");
        int colNat  = reader.GetOrdinal("nationality");
        int colBD   = reader.GetOrdinal("birth_date");
        int colDD   = reader.GetOrdinal("death_date");
        int colBio  = reader.GetOrdinal("biography");

        int count = 0;

        while (await reader.ReadAsync())
        {
            count++;

            int artistId          = reader.GetInt32(colId);
            string fullName       = reader.GetString(colName);
            string? nationality   = reader.IsDBNull(colNat) ? null : reader.GetString(colNat);
            DateTime? birthDate   = reader.IsDBNull(colBD)  ? (DateTime?)null : reader.GetDateTime(colBD);
            DateTime? deathDate   = reader.IsDBNull(colDD)  ? (DateTime?)null : reader.GetDateTime(colDD);
            string? biography     = reader.IsDBNull(colBio) ? null : reader.GetString(colBio);

            var parameters = new
            {
                artistId,
                fullName,
                nationality,
                birthDate,
                deathDate,
                biography
            };

            const string cypher = @"
                MERGE (a:Artist {artistId: $artistId})
                SET a.fullName     = $fullName,
                    a.nationality  = $nationality,
                    a.birthDate    = $birthDate,
                    a.deathDate    = $deathDate,
                    a.biography    = $biography";

            await session.RunAsync(cypher, parameters);
        }

        Console.WriteLine($"Artists migrated: {count}");
    }

    private static async Task MigrateLocations(string connStr, IAsyncSession session)
    {
        const string sql = @"SELECT location_id, name, address, room, shelf FROM locations";

        await using var mysql = new MySqlConnection(connStr);
        await mysql.OpenAsync();

        await using var cmd = new MySqlCommand(sql, mysql);
        await using var reader = await cmd.ExecuteReaderAsync();

        int colId   = reader.GetOrdinal("location_id");
        int colName = reader.GetOrdinal("name");
        int colAddr = reader.GetOrdinal("address");
        int colRoom = reader.GetOrdinal("room");
        int colShelf= reader.GetOrdinal("shelf");

        int count = 0;

        while (await reader.ReadAsync())
        {
            count++;

            int locationId      = reader.GetInt32(colId);
            string name         = reader.GetString(colName);
            string? address     = reader.IsDBNull(colAddr) ? null : reader.GetString(colAddr);
            string? room        = reader.IsDBNull(colRoom) ? null : reader.GetString(colRoom);
            string? shelf       = reader.IsDBNull(colShelf)? null : reader.GetString(colShelf);

            var parameters = new
            {
                locationId,
                name,
                address,
                room,
                shelf
            };

            const string cypher = @"
                MERGE (l:Location {locationId: $locationId})
                SET l.name    = $name,
                    l.address = $address,
                    l.room    = $room,
                    l.shelf   = $shelf";

            await session.RunAsync(cypher, parameters);
        }

        Console.WriteLine($"Locations migrated: {count}");
    }

    private static async Task MigrateOwners(string connStr, IAsyncSession session)
    {
        const string sql = @"SELECT owner_id, name, owner_type, contact_email, phone, address FROM owners";

        await using var mysql = new MySqlConnection(connStr);
        await mysql.OpenAsync();

        await using var cmd = new MySqlCommand(sql, mysql);
        await using var reader = await cmd.ExecuteReaderAsync();

        int colId    = reader.GetOrdinal("owner_id");
        int colName  = reader.GetOrdinal("name");
        int colType  = reader.GetOrdinal("owner_type");
        int colEmail = reader.GetOrdinal("contact_email");
        int colPhone = reader.GetOrdinal("phone");
        int colAddr  = reader.GetOrdinal("address");

        int count = 0;

        while (await reader.ReadAsync())
        {
            count++;

            int ownerId       = reader.GetInt32(colId);
            string name       = reader.GetString(colName);
            string ownerType  = reader.GetString(colType);
            string? email     = reader.IsDBNull(colEmail) ? null : reader.GetString(colEmail);
            string? phone     = reader.IsDBNull(colPhone) ? null : reader.GetString(colPhone);
            string? address   = reader.IsDBNull(colAddr)  ? null : reader.GetString(colAddr);

            var parameters = new
            {
                ownerId,
                name,
                ownerType,
                email,
                phone,
                address
            };

            const string cypher = @"
                MERGE (o:Owner {ownerId: $ownerId})
                SET o.name       = $name,
                    o.ownerType  = $ownerType,
                    o.email      = $email,
                    o.phone      = $phone,
                    o.address    = $address";

            await session.RunAsync(cypher, parameters);
        }

        Console.WriteLine($"Owners migrated: {count}");
    }

    private static async Task MigrateExhibitions(string connStr, IAsyncSession session)
    {
        const string sql = @"SELECT exhibition_id, name, start_date, end_date, description FROM exhibitions";

        await using var mysql = new MySqlConnection(connStr);
        await mysql.OpenAsync();

        await using var cmd = new MySqlCommand(sql, mysql);
        await using var reader = await cmd.ExecuteReaderAsync();

        int colId   = reader.GetOrdinal("exhibition_id");
        int colName = reader.GetOrdinal("name");
        int colSD   = reader.GetOrdinal("start_date");
        int colED   = reader.GetOrdinal("end_date");
        int colDesc = reader.GetOrdinal("description");

        int count = 0;

        while (await reader.ReadAsync())
        {
            count++;

            int exhibitionId       = reader.GetInt32(colId);
            string name            = reader.GetString(colName);
            DateTime? startDate    = reader.IsDBNull(colSD)   ? (DateTime?)null : reader.GetDateTime(colSD);
            DateTime? endDate      = reader.IsDBNull(colED)   ? (DateTime?)null : reader.GetDateTime(colED);
            string? description    = reader.IsDBNull(colDesc) ? null : reader.GetString(colDesc);

            var parameters = new
            {
                exhibitionId,
                name,
                startDate,
                endDate,
                description
            };

            const string cypher = @"
                MERGE (e:Exhibition {exhibitionId: $exhibitionId})
                SET e.name        = $name,
                    e.startDate   = $startDate,
                    e.endDate     = $endDate,
                    e.description = $description";

            await session.RunAsync(cypher, parameters);
        }

        Console.WriteLine($"Exhibitions migrated: {count}");
    }

    private static async Task MigrateCollections(string connStr, IAsyncSession session)
    {
        const string sql = @"SELECT collection_id, name, description FROM collections";

        await using var mysql = new MySqlConnection(connStr);
        await mysql.OpenAsync();

        await using var cmd = new MySqlCommand(sql, mysql);
        await using var reader = await cmd.ExecuteReaderAsync();

        int colId   = reader.GetOrdinal("collection_id");
        int colName = reader.GetOrdinal("name");
        int colDesc = reader.GetOrdinal("description");

        int count = 0;

        while (await reader.ReadAsync())
        {
            count++;

            int collectionId     = reader.GetInt32(colId);
            string name          = reader.GetString(colName);
            string? description  = reader.IsDBNull(colDesc) ? null : reader.GetString(colDesc);

            var parameters = new
            {
                collectionId,
                name,
                description
            };

            const string cypher = @"
                MERGE (c:Collection {collectionId: $collectionId})
                SET c.name        = $name,
                    c.description = $description";

            await session.RunAsync(cypher, parameters);
        }

        Console.WriteLine($"Collections migrated: {count}");
    }

    private static async Task MigrateArtworks(string connStr, IAsyncSession session)
    {
        const string sql = @"
            SELECT artwork_id, title, medium, year_created, dimensions,
                   notes, trigger_generated_note, created_at
            FROM artworks";

        await using var mysql = new MySqlConnection(connStr);
        await mysql.OpenAsync();

        await using var cmd = new MySqlCommand(sql, mysql);
        await using var reader = await cmd.ExecuteReaderAsync();

        int colId    = reader.GetOrdinal("artwork_id");
        int colTitle = reader.GetOrdinal("title");
        int colMed   = reader.GetOrdinal("medium");
        int colYear  = reader.GetOrdinal("year_created");
        int colDim   = reader.GetOrdinal("dimensions");
        int colNotes = reader.GetOrdinal("notes");
        int colTrig  = reader.GetOrdinal("trigger_generated_note");
        int colCreated = reader.GetOrdinal("created_at");

        int count = 0;

        while (await reader.ReadAsync())
        {
            count++;

            int artworkId       = reader.GetInt32(colId);
            string title        = reader.GetString(colTitle);
            string? medium      = reader.IsDBNull(colMed)   ? null : reader.GetString(colMed);
            int? yearCreated    = reader.IsDBNull(colYear)  ? (int?)null : reader.GetInt32(colYear);
            string? dimensions  = reader.IsDBNull(colDim)   ? null : reader.GetString(colDim);
            string? notes       = reader.IsDBNull(colNotes) ? null : reader.GetString(colNotes);
            bool triggerGen     = !reader.IsDBNull(colTrig) && reader.GetBoolean(colTrig);
            DateTime? createdAt = reader.IsDBNull(colCreated) ? (DateTime?)null : reader.GetDateTime(colCreated);

            var parameters = new
            {
                artworkId,
                title,
                medium,
                yearCreated,
                dimensions,
                notes,
                triggerGen,
                createdAt
            };

            const string cypher = @"
                MERGE (w:Artwork {artworkId: $artworkId})
                SET w.title               = $title,
                    w.medium              = $medium,
                    w.yearCreated         = $yearCreated,
                    w.dimensions          = $dimensions,
                    w.notes               = $notes,
                    w.triggerGenerated    = $triggerGen,
                    w.createdAt           = $createdAt";

            await session.RunAsync(cypher, parameters);
        }

        Console.WriteLine($"Artworks migrated: {count}");
    }

    private static async Task MigrateMedia(string connStr, IAsyncSession session)
    {
        const string sql = @"
            SELECT media_id, media_type, title, file_url, captured_date, copyright_holder, notes
            FROM media_files";

        await using var mysql = new MySqlConnection(connStr);
        await mysql.OpenAsync();

        await using var cmd = new MySqlCommand(sql, mysql);
        await using var reader = await cmd.ExecuteReaderAsync();

        int colId   = reader.GetOrdinal("media_id");
        int colType = reader.GetOrdinal("media_type");
        int colTitle= reader.GetOrdinal("title");
        int colUrl  = reader.GetOrdinal("file_url");
        int colDate = reader.GetOrdinal("captured_date");
        int colCopy = reader.GetOrdinal("copyright_holder");
        int colNotes= reader.GetOrdinal("notes");

        int count = 0;

        while (await reader.ReadAsync())
        {
            count++;

            int mediaId        = reader.GetInt32(colId);
            string mediaType   = reader.GetString(colType);
            string? title      = reader.IsDBNull(colTitle) ? null : reader.GetString(colTitle);
            string fileUrl     = reader.GetString(colUrl);
            DateTime? capDate  = reader.IsDBNull(colDate) ? (DateTime?)null : reader.GetDateTime(colDate);
            string? holder     = reader.IsDBNull(colCopy) ? null : reader.GetString(colCopy);
            string? notes      = reader.IsDBNull(colNotes)? null : reader.GetString(colNotes);

            var parameters = new
            {
                mediaId,
                mediaType,
                title,
                fileUrl,
                capDate,
                holder,
                notes
            };

            const string cypher = @"
                MERGE (m:Media {mediaId: $mediaId})
                SET m.mediaType       = $mediaType,
                    m.title           = $title,
                    m.fileUrl         = $fileUrl,
                    m.capturedDate    = $capDate,
                    m.copyrightHolder = $holder,
                    m.notes           = $notes";

            await session.RunAsync(cypher, parameters);
        }

        Console.WriteLine($"Media files migrated: {count}");
    }

    private static async Task MigrateUsers(string connStr, IAsyncSession session)
    {
        const string sql = @"SELECT UserId, UserName, Email, CreatedAt, UpdatedAt, Roles FROM users";

        await using var mysql = new MySqlConnection(connStr);
        await mysql.OpenAsync();

        await using var cmd = new MySqlCommand(sql, mysql);
        await using var reader = await cmd.ExecuteReaderAsync();

        int colId    = reader.GetOrdinal("UserId");
        int colName  = reader.GetOrdinal("UserName");
        int colEmail = reader.GetOrdinal("Email");
        int colCreated = reader.GetOrdinal("CreatedAt");
        int colUpdated = reader.GetOrdinal("UpdatedAt");
        int colRoles = reader.GetOrdinal("Roles");

        int count = 0;

        while (await reader.ReadAsync())
        {
            count++;

            int userId        = reader.GetInt32(colId);
            string userName   = reader.GetString(colName);
            string email      = reader.GetString(colEmail);
            DateTime created  = reader.GetDateTime(colCreated);
            DateTime updated  = reader.GetDateTime(colUpdated);
            string roles      = reader.GetString(colRoles);

            var parameters = new
            {
                userId,
                userName,
                email,
                created,
                updated,
                roles
            };

            const string cypher = @"
                MERGE (u:AppUser {userId: $userId})
                SET u.userName = $userName,
                    u.email    = $email,
                    u.created  = $created,
                    u.updated  = $updated,
                    u.roles    = $roles";

            await session.RunAsync(cypher, parameters);
        }

        Console.WriteLine($"Users migrated: {count}");
    }

    private static async Task MigrateRestorationNodes(string connStr, IAsyncSession session)
    {
        const string sql = @"
            SELECT restoration_id, restoration_date, conservator, restoration_type,
                   details, condition_before, condition_after, cost, currency
            FROM restorations";

        await using var mysql = new MySqlConnection(connStr);
        await mysql.OpenAsync();

        await using var cmd = new MySqlCommand(sql, mysql);
        await using var reader = await cmd.ExecuteReaderAsync();

        int colId   = reader.GetOrdinal("restoration_id");
        int colDate = reader.GetOrdinal("restoration_date");
        int colCons = reader.GetOrdinal("conservator");
        int colType = reader.GetOrdinal("restoration_type");
        int colDet  = reader.GetOrdinal("details");
        int colBefore = reader.GetOrdinal("condition_before");
        int colAfter  = reader.GetOrdinal("condition_after");
        int colCost   = reader.GetOrdinal("cost");
        int colCurr   = reader.GetOrdinal("currency");

        int count = 0;

        while (await reader.ReadAsync())
        {
            count++;

            int restorationId        = reader.GetInt32(colId);
            DateTime? restorationDate= reader.IsDBNull(colDate) ? (DateTime?)null : reader.GetDateTime(colDate);
            string? conservator      = reader.IsDBNull(colCons) ? null : reader.GetString(colCons);
            string? restorationType  = reader.IsDBNull(colType) ? null : reader.GetString(colType);
            string? details          = reader.IsDBNull(colDet)  ? null : reader.GetString(colDet);
            string? condBefore       = reader.IsDBNull(colBefore)? null : reader.GetString(colBefore);
            string? condAfter        = reader.IsDBNull(colAfter) ? null : reader.GetString(colAfter);
            decimal? cost            = reader.IsDBNull(colCost) ? (decimal?)null : reader.GetDecimal(colCost);
            string? currency         = reader.IsDBNull(colCurr) ? null : reader.GetString(colCurr);

            var parameters = new
            {
                restorationId,
                restorationDate,
                conservator,
                restorationType,
                details,
                condBefore,
                condAfter,
                cost,
                currency
            };

            const string cypher = @"
                MERGE (r:Restoration {restorationId: $restorationId})
                SET r.restorationDate = $restorationDate,
                    r.conservator     = $conservator,
                    r.restorationType = $restorationType,
                    r.details         = $details,
                    r.conditionBefore = $condBefore,
                    r.conditionAfter  = $condAfter,
                    r.cost            = $cost,
                    r.currency        = $currency";

            await session.RunAsync(cypher, parameters);
        }

        Console.WriteLine($"Restoration nodes migrated: {count}");
    }

    private static async Task MigrateTransactionNodes(string connStr, IAsyncSession session)
    {
        const string sql = @"
            SELECT transaction_id, artwork_id, txn_date, txn_type,
                   price, currency, notes
            FROM transactions";

        await using var mysql = new MySqlConnection(connStr);
        await mysql.OpenAsync();

        await using var cmd = new MySqlCommand(sql, mysql);
        await using var reader = await cmd.ExecuteReaderAsync();

        int colId   = reader.GetOrdinal("transaction_id");
        int colDate = reader.GetOrdinal("txn_date");
        int colType = reader.GetOrdinal("txn_type");
        int colPrice= reader.GetOrdinal("price");
        int colCurr = reader.GetOrdinal("currency");
        int colNotes= reader.GetOrdinal("notes");

        int count = 0;

        while (await reader.ReadAsync())
        {
            count++;

            int transactionId       = reader.GetInt32(colId);
            DateTime txnDate        = reader.GetDateTime(colDate);
            string txnType          = reader.GetString(colType);
            decimal? price          = reader.IsDBNull(colPrice) ? (decimal?)null : reader.GetDecimal(colPrice);
            string? currency        = reader.IsDBNull(colCurr) ? null : reader.GetString(colCurr);
            string? notes           = reader.IsDBNull(colNotes)? null : reader.GetString(colNotes);

            var parameters = new
            {
                transactionId,
                txnDate,
                txnType,
                price,
                currency,
                notes
            };

            const string cypher = @"
                MERGE (t:Transaction {transactionId: $transactionId})
                SET t.txnDate  = $txnDate,
                    t.txnType  = $txnType,
                    t.price    = $price,
                    t.currency = $currency,
                    t.notes    = $notes";

            await session.RunAsync(cypher, parameters);
        }

        Console.WriteLine($"Transaction nodes migrated: {count}");
    }

    // =========================
    // Relationship migrations
    // =========================

    private static async Task LinkArtworksToArtists(string connStr, IAsyncSession session)
    {
        const string sql = @"SELECT artwork_id, primary_artist_id FROM artworks WHERE primary_artist_id IS NOT NULL";

        await using var mysql = new MySqlConnection(connStr);
        await mysql.OpenAsync();

        await using var cmd = new MySqlCommand(sql, mysql);
        await using var reader = await cmd.ExecuteReaderAsync();

        int colArtwork = reader.GetOrdinal("artwork_id");
        int colArtist  = reader.GetOrdinal("primary_artist_id");

        int count = 0;

        while (await reader.ReadAsync())
        {
            count++;

            int artworkId = reader.GetInt32(colArtwork);
            int artistId  = reader.GetInt32(colArtist);

            var parameters = new { artworkId, artistId };

            const string cypher = @"
                MATCH (w:Artwork {artworkId: $artworkId})
                MATCH (a:Artist  {artistId:  $artistId})
                MERGE (a)-[:CREATED]->(w)";

            await session.RunAsync(cypher, parameters);
        }

        Console.WriteLine($"Artwork–Artist relationships created: {count}");
    }

    private static async Task LinkArtworksToLocations(string connStr, IAsyncSession session)
    {
        const string sql = @"SELECT artwork_id, current_location_id FROM artworks WHERE current_location_id IS NOT NULL";

        await using var mysql = new MySqlConnection(connStr);
        await mysql.OpenAsync();

        await using var cmd = new MySqlCommand(sql, mysql);
        await using var reader = await cmd.ExecuteReaderAsync();

        int colArtwork = reader.GetOrdinal("artwork_id");
        int colLoc     = reader.GetOrdinal("current_location_id");

        int count = 0;

        while (await reader.ReadAsync())
        {
            count++;

            int artworkId  = reader.GetInt32(colArtwork);
            int locationId = reader.GetInt32(colLoc);

            var parameters = new { artworkId, locationId };

            const string cypher = @"
                MATCH (w:Artwork  {artworkId:  $artworkId})
                MATCH (l:Location {locationId: $locationId})
                MERGE (w)-[:CURRENT_LOCATION]->(l)";

            await session.RunAsync(cypher, parameters);
        }

        Console.WriteLine($"Artwork–Location relationships created: {count}");
    }

    private static async Task LinkArtworksToOwners(string connStr, IAsyncSession session)
    {
        const string sql = @"SELECT artwork_id, current_owner_id FROM artworks WHERE current_owner_id IS NOT NULL";

        await using var mysql = new MySqlConnection(connStr);
        await mysql.OpenAsync();

        await using var cmd = new MySqlCommand(sql, mysql);
        await using var reader = await cmd.ExecuteReaderAsync();

        int colArtwork = reader.GetOrdinal("artwork_id");
        int colOwner   = reader.GetOrdinal("current_owner_id");

        int count = 0;

        while (await reader.ReadAsync())
        {
            count++;

            int artworkId = reader.GetInt32(colArtwork);
            int ownerId   = reader.GetInt32(colOwner);

            var parameters = new { artworkId, ownerId };

            const string cypher = @"
                MATCH (w:Artwork {artworkId: $artworkId})
                MATCH (o:Owner   {ownerId:   $ownerId})
                MERGE (w)-[:CURRENT_OWNER]->(o)";

            await session.RunAsync(cypher, parameters);
        }

        Console.WriteLine($"Artwork–Owner relationships created: {count}");
    }

    private static async Task MigrateCollectionItemsRels(string connStr, IAsyncSession session)
    {
        const string sql = @"SELECT collection_id, artwork_id, date_added, item_notes FROM collection_items";

        await using var mysql = new MySqlConnection(connStr);
        await mysql.OpenAsync();

        await using var cmd = new MySqlCommand(sql, mysql);
        await using var reader = await cmd.ExecuteReaderAsync();

        int colColl  = reader.GetOrdinal("collection_id");
        int colArt   = reader.GetOrdinal("artwork_id");
        int colDate  = reader.GetOrdinal("date_added");
        int colNotes = reader.GetOrdinal("item_notes");

        int count = 0;

        while (await reader.ReadAsync())
        {
            count++;

            int collectionId    = reader.GetInt32(colColl);
            int artworkId       = reader.GetInt32(colArt);
            DateTime? dateAdded = reader.IsDBNull(colDate) ? (DateTime?)null : reader.GetDateTime(colDate);
            string? notes       = reader.IsDBNull(colNotes)? null : reader.GetString(colNotes);

            var parameters = new
            {
                collectionId,
                artworkId,
                dateAdded,
                notes
            };

            const string cypher = @"
                MATCH (c:Collection {collectionId: $collectionId})
                MATCH (w:Artwork    {artworkId:    $artworkId})
                MERGE (c)-[r:CONTAINS]->(w)
                SET r.dateAdded = $dateAdded,
                    r.notes     = $notes";

            await session.RunAsync(cypher, parameters);
        }

        Console.WriteLine($"Collection–Artwork relationships created: {count}");
    }

    private static async Task MigrateExhibitionArtworksRels(string connStr, IAsyncSession session)
    {
        const string sql = @"SELECT exhibition_id, artwork_id, display_label, notes FROM exhibition_artworks";

        await using var mysql = new MySqlConnection(connStr);
        await mysql.OpenAsync();

        await using var cmd = new MySqlCommand(sql, mysql);
        await using var reader = await cmd.ExecuteReaderAsync();

        int colExh   = reader.GetOrdinal("exhibition_id");
        int colArt   = reader.GetOrdinal("artwork_id");
        int colLabel = reader.GetOrdinal("display_label");
        int colNotes = reader.GetOrdinal("notes");

        int count = 0;

        while (await reader.ReadAsync())
        {
            count++;

            int exhibitionId = reader.GetInt32(colExh);
            int artworkId    = reader.GetInt32(colArt);
            string? label    = reader.IsDBNull(colLabel) ? null : reader.GetString(colLabel);
            string? notes    = reader.IsDBNull(colNotes) ? null : reader.GetString(colNotes);

            var parameters = new
            {
                exhibitionId,
                artworkId,
                label,
                notes
            };

            const string cypher = @"
                MATCH (e:Exhibition {exhibitionId: $exhibitionId})
                MATCH (w:Artwork    {artworkId:    $artworkId})
                MERGE (w)-[r:ON_EXHIBITION]->(e)
                SET r.displayLabel = $label,
                    r.notes        = $notes";

            await session.RunAsync(cypher, parameters);
        }

        Console.WriteLine($"Exhibition–Artwork relationships created: {count}");
    }

    private static async Task MigrateMediaRelations(string connStr, IAsyncSession session)
    {
        // artwork_id + artist_id are optional
        const string sql = @"SELECT media_id, artwork_id, artist_id FROM media_files";

        await using var mysql = new MySqlConnection(connStr);
        await mysql.OpenAsync();

        await using var cmd = new MySqlCommand(sql, mysql);
        await using var reader = await cmd.ExecuteReaderAsync();

        int colMedia = reader.GetOrdinal("media_id");
        int colArt   = reader.GetOrdinal("artwork_id");
        int colArtist= reader.GetOrdinal("artist_id");

        int countArt = 0;
        int countArtist = 0;

        while (await reader.ReadAsync())
        {
            int mediaId = reader.GetInt32(colMedia);

            if (!reader.IsDBNull(colArt))
            {
                countArt++;
                int artworkId = reader.GetInt32(colArt);
                var pArtwork = new { mediaId, artworkId };

                const string cypherArtwork = @"
                    MATCH (m:Media   {mediaId:  $mediaId})
                    MATCH (w:Artwork {artworkId: $artworkId})
                    MERGE (w)-[:HAS_MEDIA]->(m)";

                await session.RunAsync(cypherArtwork, pArtwork);
            }

            if (!reader.IsDBNull(colArtist))
            {
                countArtist++;
                int artistId = reader.GetInt32(colArtist);
                var pArtist = new { mediaId, artistId };

                const string cypherArtist = @"
                    MATCH (m:Media   {mediaId:  $mediaId})
                    MATCH (a:Artist  {artistId: $artistId})
                    MERGE (a)-[:FEATURED_IN_MEDIA]->(m)";

                await session.RunAsync(cypherArtist, pArtist);
            }
        }

        Console.WriteLine($"Media relationships created: HAS_MEDIA={countArt}, FEATURED_IN_MEDIA={countArtist}");
    }

    private static async Task MigrateOwnershipRels(string connStr, IAsyncSession session)
    {
        const string sql = @"
            SELECT ownership_id, artwork_id, owner_id,
                   acquired_date, relinquished_date, source_document, notes
            FROM ownership";

        await using var mysql = new MySqlConnection(connStr);
        await mysql.OpenAsync();

        await using var cmd = new MySqlCommand(sql, mysql);
        await using var reader = await cmd.ExecuteReaderAsync();

        int colOwn  = reader.GetOrdinal("ownership_id");
        int colArt  = reader.GetOrdinal("artwork_id");
        int colOwner= reader.GetOrdinal("owner_id");
        int colAcq  = reader.GetOrdinal("acquired_date");
        int colRel  = reader.GetOrdinal("relinquished_date");
        int colSrc  = reader.GetOrdinal("source_document");
        int colNotes= reader.GetOrdinal("notes");

        int count = 0;

        while (await reader.ReadAsync())
        {
            count++;

            int ownershipId       = reader.GetInt32(colOwn);
            int artworkId         = reader.GetInt32(colArt);
            int ownerId           = reader.GetInt32(colOwner);
            DateTime? acquired    = reader.IsDBNull(colAcq) ? (DateTime?)null : reader.GetDateTime(colAcq);
            DateTime? relinquished= reader.IsDBNull(colRel) ? (DateTime?)null : reader.GetDateTime(colRel);
            string? sourceDoc     = reader.IsDBNull(colSrc) ? null : reader.GetString(colSrc);
            string? notes         = reader.IsDBNull(colNotes) ? null : reader.GetString(colNotes);

            var parameters = new
            {
                ownershipId,
                artworkId,
                ownerId,
                acquired,
                relinquished,
                sourceDoc,
                notes
            };

            const string cypher = @"
                MATCH (w:Artwork {artworkId: $artworkId})
                MATCH (o:Owner   {ownerId:   $ownerId})
                MERGE (w)-[r:OWNED_BY {ownershipId: $ownershipId}]->(o)
                SET r.acquiredDate     = $acquired,
                    r.relinquishedDate = $relinquished,
                    r.sourceDocument   = $sourceDoc,
                    r.notes            = $notes";

            await session.RunAsync(cypher, parameters);
        }

        Console.WriteLine($"Ownership relationships created: {count}");
    }

    private static async Task LinkRestorations(string connStr, IAsyncSession session)
    {
        const string sql = @"SELECT restoration_id, artwork_id FROM restorations";

        await using var mysql = new MySqlConnection(connStr);
        await mysql.OpenAsync();

        await using var cmd = new MySqlCommand(sql, mysql);
        await using var reader = await cmd.ExecuteReaderAsync();

        int colRest = reader.GetOrdinal("restoration_id");
        int colArt  = reader.GetOrdinal("artwork_id");

        int count = 0;

        while (await reader.ReadAsync())
        {
            count++;

            int restorationId = reader.GetInt32(colRest);
            int artworkId     = reader.GetInt32(colArt);

            var parameters = new { restorationId, artworkId };

            const string cypher = @"
                MATCH (r:Restoration {restorationId: $restorationId})
                MATCH (w:Artwork     {artworkId:    $artworkId})
                MERGE (w)-[:HAS_RESTORATION]->(r)";

            await session.RunAsync(cypher, parameters);
        }

        Console.WriteLine($"Restoration relationships created: {count}");
    }

    private static async Task LinkTransactions(string connStr, IAsyncSession session)
    {
        // link Transaction nodes to Artwork + Owners
        const string sql = @"
            SELECT transaction_id, artwork_id, from_owner_id, to_owner_id
            FROM transactions";

        await using var mysql = new MySqlConnection(connStr);
        await mysql.OpenAsync();

        await using var cmd = new MySqlCommand(sql, mysql);
        await using var reader = await cmd.ExecuteReaderAsync();

        int colId    = reader.GetOrdinal("transaction_id");
        int colArt   = reader.GetOrdinal("artwork_id");
        int colFrom  = reader.GetOrdinal("from_owner_id");
        int colTo    = reader.GetOrdinal("to_owner_id");

        int count = 0;
        int countFrom = 0;
        int countTo = 0;

        while (await reader.ReadAsync())
        {
            count++;

            int transactionId = reader.GetInt32(colId);
            int artworkId     = reader.GetInt32(colArt);

            // link transaction -> artwork
            var pArt = new { transactionId, artworkId };

            const string cypherArt = @"
                MATCH (t:Transaction {transactionId: $transactionId})
                MATCH (w:Artwork     {artworkId:    $artworkId})
                MERGE (t)-[:FOR_ARTWORK]->(w)";

            await session.RunAsync(cypherArt, pArt);

            // optional from_owner
            if (!reader.IsDBNull(colFrom))
            {
                countFrom++;
                int fromOwnerId = reader.GetInt32(colFrom);
                var pFrom = new { transactionId, fromOwnerId };

                const string cypherFrom = @"
                    MATCH (t:Transaction {transactionId: $transactionId})
                    MATCH (o:Owner       {ownerId:       $fromOwnerId})
                    MERGE (o)-[:FROM_OWNER]->(t)";

                await session.RunAsync(cypherFrom, pFrom);
            }

            // optional to_owner
            if (!reader.IsDBNull(colTo))
            {
                countTo++;
                int toOwnerId = reader.GetInt32(colTo);
                var pTo = new { transactionId, toOwnerId };

                const string cypherTo = @"
                    MATCH (t:Transaction {transactionId: $transactionId})
                    MATCH (o:Owner       {ownerId:       $toOwnerId})
                    MERGE (t)-[:TO_OWNER]->(o)";

                await session.RunAsync(cypherTo, pTo);
            }
        }

        Console.WriteLine($"Transaction relationships created: FOR_ARTWORK={count}, FROM_OWNER={countFrom}, TO_OWNER={countTo}");
    }
}
