namespace HouseDemo.Backend.Models;

public class LoginRequest
{
    public string? Email { get; set; }
    public string? Password { get; set; }
}

public class AuthResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? Token { get; set; }
    public int? ExpiresInSeconds { get; set; }
}

public class TokenClaims
{
    public int UserId { get; set; }
    public string? Email { get; set; }
    public string? Role { get; set; }
}
