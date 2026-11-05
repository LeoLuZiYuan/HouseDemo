using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace HouseDemo.Backend.Data;

public class AuthMiddleware
{
    private readonly RequestDelegate _next;
    private readonly string _jwtSecret;

    public AuthMiddleware(RequestDelegate next, IConfiguration config)
    {
        _next = next;
        _jwtSecret = config["Jwt:Secret"] ?? throw new InvalidOperationException("JWT Secret not configured");
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path;

        // Skip auth check for login endpoint
        if (path.StartsWithSegments("/api/auth/login"))
        {
            await _next(context);
            return;
        }

        // Check auth for protected routes (/api/admin/*)
        if (path.StartsWithSegments("/api/admin"))
        {
            var authHeader = context.Request.Headers["Authorization"].ToString();

            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsJsonAsync(new { message = "Unauthorized: Missing token" });
                return;
            }

            var token = authHeader["Bearer ".Length..];

            try
            {
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSecret));
                var handler = new JwtSecurityTokenHandler();
                handler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = key,
                    ValidateIssuer = true,
                    ValidIssuer = "housedemoll.com",
                    ValidateAudience = true,
                    ValidAudience = "housedemoll.com",
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                context.Items["User"] = validatedToken;
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsJsonAsync(new { message = $"Unauthorized: {ex.Message}" });
                return;
            }
        }

        await _next(context);
    }
}
