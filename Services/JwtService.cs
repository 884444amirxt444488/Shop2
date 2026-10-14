
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;

public class JwtService
{
    private readonly IConfiguration _configuration;

    public JwtService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateAccessToken(int UserId)
    {
        string Key = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("Key not found");
        string Issuer = _configuration["Jwt:Issuer"] ?? throw new InvalidOperationException("Issuer not found");
        string Audience = _configuration["Jwt:Audience"] ?? throw new InvalidOperationException("Audience not found");
        int ExpiresMinutes = _configuration.GetValue<int?>("Jwt:ExpiresMinutes") ?? throw new InvalidOperationException("ExpiresMinutes not found");

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, UserId.ToString())
        };

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Key));

        var credentials = new SigningCredentials(
            securityKey,
            SecurityAlgorithms.HmacSha256
        );

        var AccessToken = new JwtSecurityToken(
            issuer: Issuer,
            audience: Audience,
            expires: DateTime.UtcNow.AddDays(ExpiresMinutes),
            claims: claims,
            signingCredentials: credentials
        );
        
        return new JwtSecurityTokenHandler().WriteToken(AccessToken);
    } 
    public string GenerateRefreshToken()
    {
        byte[] bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes);
    }
}





