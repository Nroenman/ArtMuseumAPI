using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ArtMuseumAPI.DTO.User;
using ArtMuseumAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace ArtMuseumAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController(IUserService userService, IConfiguration configuration) : ControllerBase
{
    [HttpPost("login")]
    public ActionResult<object> Login([FromBody] AuthInput input)
    {
        var user = userService.Authenticate(input.Email, input.Password);
        if (user == null)
            return Unauthorized(new { Message = "Incorrect email or password." });

        // roles: "Admin,User" etc.
        var roles = (user.Roles ?? "")
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        var keyBytes = Encoding.UTF8.GetBytes(configuration["AppSettings:Token"]!);
        var creds = new SigningCredentials(new SymmetricSecurityKey(keyBytes), SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new("name", user.UserName),
        };
        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var jwt = new JwtSecurityToken(
            issuer:   configuration["AppSettings:Issuer"],
            audience: configuration["AppSettings:Audience"],
            claims:   claims,
            expires:  DateTime.UtcNow.AddHours(1),
            signingCredentials: creds
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(jwt);
        return Ok(new { token = tokenString });
    }
}