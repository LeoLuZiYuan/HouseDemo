using HouseDemo.Backend.Data;
using HouseDemo.Backend.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<Database>();
builder.Services.AddSingleton<PropertyRepository>();
builder.Services.AddSingleton<AuthService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

app.UseCors("AllowAll");
app.UseMiddleware<AuthMiddleware>();

// Auth endpoints
app.MapPost("/api/auth/login", LoginHandler)
    .WithName("Login");

// Admin properties (protected)
var adminGroup = app.MapGroup("/api/admin/properties")
    .WithName("AdminProperties");

adminGroup.MapGet("/", GetAllPropertiesAdmin)
    .WithName("GetAllPropertiesAdmin");

// Public properties (from Phase 1)
var group = app.MapGroup("/api/properties").WithName("Properties");

group.MapGet("/", GetAllProperties)
    .WithName("GetAllProperties");

group.MapGet("/{id}", GetPropertyById)
    .WithName("GetPropertyById");

// Handlers
async Task<IResult> LoginHandler(LoginRequest request, AuthService authService)
{
    var (success, token, message) = authService.Login(request.Email, request.Password);
    
    if (!success)
        return Results.Unauthorized();

    return Results.Ok(new AuthResponse
    {
        Success = true,
        Message = message,
        Token = token,
        ExpiresInSeconds = 86400
    });
}

async Task<IResult> GetAllPropertiesAdmin(PropertyRepository repo)
{
    var properties = await repo.GetAllPropertiesAsync();
    return Results.Ok(properties);
}

async Task<IResult> GetAllProperties(PropertyRepository repo)
{
    var properties = await repo.GetAllPropertiesAsync();
    return Results.Ok(properties);
}

async Task<IResult> GetPropertyById(int id, PropertyRepository repo)
{
    var property = await repo.GetPropertyByIdAsync(id);
    if (property == null)
        return Results.NotFound();
    return Results.Ok(property);
}

app.Run();
