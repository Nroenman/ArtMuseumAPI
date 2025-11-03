using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ArtMuseumAPI.Models;
using ArtMuseumAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// --- MVC & DI ---
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IUserService, UserService>();


// --- DB ---
var cs = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(opts =>
    opts.UseMySql(cs, ServerVersion.AutoDetect(cs)));

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
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "ArtMuseumAPI", Version = "v1" });

    // Make Swagger UI call the http base directly to avoid redirect issues
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
        [ new OpenApiSecurityScheme
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
        c.RoutePrefix = string.Empty; // Swagger on /
    });
}

// Keep HTTPS redirect OFF to avoid header loss during redirects in dev
// app.UseHttpsRedirection();

app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
