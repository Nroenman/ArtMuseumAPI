using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using MysqlMongodbMigrator.Services;
using MysqlMongodbMigrator.Data;
using Microsoft.EntityFrameworkCore;

namespace MysqlMongodbMigrator
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddUserSecrets<Program>(optional: true)
                .AddEnvironmentVariables()
                .Build();

            string? mySqlConnectionString = config.GetConnectionString("DefaultConnection");

            var mongoSection = config.GetSection("MongoDB");
            string? mongoConnectionString = mongoSection.GetValue<string>("ConnectionString");
            string? mongoDatabaseName = mongoSection.GetValue<string>("DatabaseName");

            if (string.IsNullOrEmpty(mySqlConnectionString) ||
                string.IsNullOrEmpty(mongoConnectionString) ||
                string.IsNullOrEmpty(mongoDatabaseName))
            {
                Console.WriteLine("Connection strings are not set in configuration.");
                return;
            }

            var serviceProvider = new ServiceCollection()
                .AddDbContext<KunstMuseumDbContext>(options =>
                    options.UseMySql(
                        mySqlConnectionString,
                        new MySqlServerVersion(new Version(8, 0, 0))
                    ))
                .AddScoped<MigrationService>()
                .AddSingleton(provider => new MongoDbService(mongoConnectionString, mongoDatabaseName))
                .BuildServiceProvider();

            var migrationService = serviceProvider.GetRequiredService<MigrationService>();
            await migrationService.MigrateAsync();
        }
    }
}