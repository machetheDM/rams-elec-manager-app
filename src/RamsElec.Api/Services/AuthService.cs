using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using RamsElec.Shared.DTOs;

namespace RamsElec.Api.Services;

public class AuthService
{
    private readonly IConfiguration _config;

    public AuthService(IConfiguration config)
    {
        _config = config;
    }

    public AuthResponseDto GenerateToken(string email, string displayName, string role)
    {
        var key = _config["Jwt:Key"] ?? "dev-jwt-key-change-in-production-minimum-32-chars";
        var issuer = _config["Jwt:Issuer"] ?? "RamsElec.Api";
        var audience = _config["Jwt:Audience"] ?? "RamsElec.App";
        var expiresAt = DateTime.UtcNow.AddHours(24);

        var claims = new[]
        {
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.Name, displayName),
            new Claim(ClaimTypes.Role, role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        return new AuthResponseDto
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            DisplayName = displayName,
            Role = role,
            ExpiresAt = expiresAt
        };
    }
}
