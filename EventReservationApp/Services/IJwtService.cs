using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.Extensions.Configuration;
using EventReservations.Models;

public interface IJwtService
{
    string GenerateToken(User user);
}

public class JwtService : IJwtService
{
    private readonly IConfiguration _configuration;
    private readonly byte[] _key;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly double _expireMinutes;

    public JwtService(IConfiguration configuration)
    {
        _configuration = configuration;
        var jwt = _configuration.GetSection("Jwt");
        _key = Encoding.ASCII.GetBytes(jwt["Key"]);
        _issuer = jwt["Issuer"] ?? string.Empty;
        _audience = jwt["Audience"] ?? string.Empty;
        _expireMinutes = double.Parse(jwt["ExpireMinutes"] ?? "60");
    }

    public string GenerateToken(User user)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Role, user.Role ?? "User")
        };

        var creds = new SigningCredentials(new SymmetricSecurityKey(_key), SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_expireMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
