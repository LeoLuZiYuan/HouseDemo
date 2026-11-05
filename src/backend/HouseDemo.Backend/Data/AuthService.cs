using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BCrypt.Net;
using Microsoft.IdentityModel.Tokens;

namespace HouseDemo.Backend.Data;

public class AuthService
{
    private readonly IConfiguration _config;
    private readonly string _jwtSecret;
    private readonly string _landlordemail;
    private readonly string _landlorPassword;

    public AuthService(IConfiguration config)
    {
        _config = config;
        _jwtSecret = config["Jwt:Secret"] ?? throw new InvalidOperationException("JWT Secret not configured");
        _landlordemail = config["Landlord:Email"] ?? "landlord@housedemoll.com";
        _landlorPassword = config["Landlord:PasswordHash"] ?? GetPasswordHash("demo123456");
    }

    public (bool success, string? token, string message) Login(string? email, string? password)
    {
        // Simple single landlord check for MVP
        if (email != _landlordemail)
            return (false, null, "Invalid email or password");

        if (password == null || !BCrypt.Net.BCrypt.Verify(password, _landlorPassword))
            return (false, null, "Invalid email or password");

        var token = GenerateJwtToken(email);
        return (true, token, "Login successful");
    }

    private string GenerateJwtToken(string email)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSecret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddHours(24);

        var claims = new[]
        {
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.Role, "Landlord")
        };

        var token = new JwtSecurityToken(
            issuer: "housedemoll.com",
            audience: "housedemoll.com",
            claims: claims,
            expires: expires,
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GetPasswordHash(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }
}
