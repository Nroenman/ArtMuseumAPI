using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace ArtMuseumAPI.Middleware;

public class JwtMiddleware(RequestDelegate next, IConfiguration configuration, ILogger<JwtMiddleware> logger)
{
    public async Task Invoke(HttpContext context)
    {
        var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
        if (!string.IsNullOrEmpty(token))
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(configuration["AppSettings:Token"]);

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidIssuer = configuration["AppSettings:Issuer"],
                    ValidAudience = configuration["AppSettings:Audience"],
                    ClockSkew = TimeSpan.Zero
                };

                var principal =
                    tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
                var userId = principal.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
                var roles = principal.Claims.FirstOrDefault(c => c.Type == "roles")?.Value;

                if (userId != null)
                {
                    context.Items["User"] = userId; 
                }

                if (roles != null)
                {
                    context.Items["Roles"] = roles.Split(','); 
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"JWT Token validation failed: {ex.Message}");
            }
        }

        await next(context);
    }
}