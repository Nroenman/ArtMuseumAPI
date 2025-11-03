using System.Text;
using ArtMuseumAPI.Models;
using ArtMuseumAPI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;

var builder = WebApplication.CreateBuilder(args);

// ------------------------------------------------------------
// 1. Add core services
// ------------------------------------------------------------
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpContextAccessor();

// ------------------------------------------------------------
// 2. Register application services
// ------------------------------------------------------------
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<IUserService, UserService>();

// ------------------------------------------------------------
// 3. Database setup (MySQL)
// ------------------------------------------------------------
var cs = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(opts =>
    opts.UseMySql(cs, ServerVersion.AutoDetect(cs)));

// ------------------------------------------------------------
// 4. CORS policy
// ------------------------------------------------------------
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// ------------------------------------------------------------
// 5. Swagger (with JWT Auth support)
// ------------------------------------------------------------
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ArtMuseumAPI",
        Version = "v1",
        Description = "API for Art Museum project with JWT authentication"
    });

    // --- Add Bearer Security ---
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Enter: Bearer {your token}"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    // Optional: adds [Authorize] annotation support in Swagger UI
    options.OperationFilter<SecurityRequirementsOperationFilter>();
});

// ------------------------------------------------------------
// 6. JWT Authentication
// ------------------------------------------------------------
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false; // true in production
        options.SaveToken = true;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["AppSettings:Token"]!)
            ),

            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["AppSettings:Issuer"],

            ValidateAudience = true,
            ValidAudience = builder.Configuration["AppSettings:Audience"],

            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,

            // IMPORTANT: your token uses "roles" and "sub"
            RoleClaimType = "roles",
            NameClaimType = "sub"
        };
    });

// ------------------------------------------------------------
// 7. Build the app
// ------------------------------------------------------------
var app = builder.Build();

// ------------------------------------------------------------
// 8. Configure middleware pipeline
// ------------------------------------------------------------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "ArtMuseumAPI v1");
        options.RoutePrefix = string.Empty; // Swagger at root
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");

// ORDER MATTERS:
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
