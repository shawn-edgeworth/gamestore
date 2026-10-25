using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using GameStore.Api.Models;
using Microsoft.IdentityModel.Tokens;

namespace GameStore.Api.Services;

public class JwtTokenService
{
    private readonly IConfiguration configuration;

    public JwtTokenService(IConfiguration configuration)
    {
        this.configuration = configuration;
    }

    public string CreateToken(User user)
    {
        var key = configuration["Jwt:Key"];
        var bytes = Encoding.UTF8.GetBytes(key);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier,user.Id.ToString()),
            new Claim(ClaimTypes.Email,user.Email),
            new Claim(ClaimTypes.Role,user.Role),
        };

        var symmetricSecurityKey = new SymmetricSecurityKey(bytes);
        var signingCredentials = new SigningCredentials(symmetricSecurityKey,
        SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            "GameStore", "GameStore.Api", claims,
            expires: DateTime.UtcNow.AddHours(2),
            signingCredentials: signingCredentials);

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
        return tokenString;
    }
}