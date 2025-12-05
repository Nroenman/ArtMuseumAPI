using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ArtMuseumAPI.Models;
using ArtMuseumAPI.Models.Mongo;
using ArtMuseumAPI.Models.Neo4j;
using ArtMuseumAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
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

// --- DB: MySQL ---
var cs = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(opts =>
    opts.UseMySql(cs, ServerVersion.AutoDetect(cs)));


// === NEW: MongoDB DI ===
builder.Services.Configure<MongoSettings>(
    builder.Configuration.GetSection("MongoSettings"));

builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var settings = sp.GetRequiredService<IOptions<MongoSettings>>().Value;
    return new MongoClient(settings.ConnectionString);
});


// === NEW: Neo4j DI ===
builder.Services.Configure<Neo4JSettings>(
    builder.Configuration.GetSection("Neo4jSettings"));

builder.Services.AddSingleton<IDriver>(sp =>
{
    var cfg = sp.GetRequiredService<IOptions<Neo4JSettings>>().Value;
    return GraphDatabase.Driver(cfg.Uri, AuthTokens.Basic(cfg.User, cfg.Password));
});


// --- CORS (allow all) ---
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", p => p
        .AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader());
});

// --- JWT Auth (VALIDATED, matches AuthController) ---
var signingKeyBytes = Encoding.UTF8.GetBytes(builder.Configuration["AppSettings:Token"]!);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,

            ValidIssuer = builder.Configuration["AppSettings:Issuer"],
            ValidAudience = builder.Configuration["AppSettings:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(signingKeyBytes),

            RoleClaimType = ClaimTypes.Role,
            NameClaimType = JwtRegisteredClaimNames.Sub,
            ClockSkew = TimeSpan.FromMinutes(2)
        };
    });

// --- Swagger ---
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

    var scheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Paste your JWT (no 'Bearer ' prefix)."
    };
    options.AddSecurityDefinition("Bearer", scheme);
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        [new OpenApiSecurityScheme
        { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } }
        ] = Array.Empty<string>()
    });
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
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
