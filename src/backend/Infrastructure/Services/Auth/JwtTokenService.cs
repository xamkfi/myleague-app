using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Application.Configuration;
using Application.Interfaces.Auth;
using Domain.Entities.Common;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace MyLeague.Infrastructure.Services.Auth;

/// <summary>
/// JWT token service for generating access tokens and refresh tokens
/// </summary>
public class JwtTokenService : IJwtTokenService
{
    private readonly JwtConfiguration _jwtConfig;

    public JwtTokenService(IOptions<JwtConfiguration> jwtConfig)
    {
        _jwtConfig = jwtConfig.Value;
    }

    public (string Token, DateTime ExpiresAt) GenerateAccessToken(User user, int accessTokenExpirationMinutes)
    {
        DateTime expiresAt = DateTime.UtcNow.AddMinutes(accessTokenExpirationMinutes);

        List<Claim> claims = new()
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("personId", user.PersonId.ToString())
        };

        // Add user role (SystemAdmin / ClubAdmin)
        claims.Add(new Claim(ClaimTypes.Role, user.Role.ToString()));

        // Add person role if available
        if (user.Person != null)
        {
            claims.Add(new Claim("personRole", user.Person.role.ToString()));
        }

        SymmetricSecurityKey key = new(Encoding.UTF8.GetBytes(_jwtConfig.SecretKey));
        SigningCredentials credentials = new(key, SecurityAlgorithms.HmacSha256);

        JwtSecurityToken token = new(
            issuer: _jwtConfig.Issuer,
            audience: _jwtConfig.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        string tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        return (tokenString, expiresAt);
    }

    public string GenerateRefreshToken()
    {
        byte[] randomBytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(randomBytes);
    }

    public string HashToken(string token)
    {
        byte[] tokenBytes = Encoding.UTF8.GetBytes(token);
        byte[] hashBytes = SHA256.HashData(tokenBytes);
        return Convert.ToBase64String(hashBytes);
    }
}
