using ArtMuseumAPI.Models;
using ArtMuseumAPI.Models.Mongo;
using ArtMuseumAPI.Models.Neo4j;
using ArtMuseumAPI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using Neo4j.Driver;
using ServerVersion = Microsoft.EntityFrameworkCore.ServerVersion;


var builder = WebApplication.CreateBuilder(args);

// --- MVC & DI ---
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ICollectionsService , CollectionsService>();

//MySQL
var cs = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(opts =>
    opts.UseMySql(cs, ServerVersion.AutoDetect(cs)));

//MongoDB
builder.Services.Configure<MongoSettings>(
    builder.Configuration.GetSection("MongoSettings"));

builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var settings = sp.GetRequiredService<IOptions<MongoSettings>>().Value;
    return new MongoClient(settings.ConnectionString);
});

//Neo4j
builder.Services.Configure<Neo4JSettings>(
    builder.Configuration.GetSection("Neo4jSettings"));

builder.Services.AddSingleton<IDriver>(sp =>
{
    var cfg = sp.GetRequiredService<IOptions<Neo4JSettings>>().Value;
    return GraphDatabase.Driver(cfg.Uri, AuthTokens.Basic(cfg.User, cfg.Password));
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", p => p
        .AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader());
});

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ArtMuseumAPI",
        Version = "v1"
    });

    options.TagActionsBy(api =>
    {
        return new[]
        {
            api.GroupName ?? api.ActionDescriptor.RouteValues["controller"]!
        };
    });

    options.DocInclusionPredicate((docName, apiDesc) => true);
    options.AddServer(new OpenApiServer { Url = "http://localhost:5133" });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "ArtMuseumAPI v1");
        c.RoutePrefix = string.Empty;
    });
}

app.UseCors("AllowAll");
app.MapControllers();

app.Run();
