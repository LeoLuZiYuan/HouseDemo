using HouseDemo.Backend.Data;
using HouseDemo.Backend.Models;

var builder = WebApplication.CreateBuilder(args);

// Configure port from PORT environment variable (Render) or default to 8080
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://+:{port}");

builder.Services.AddSingleton<Database>();
builder.Services.AddSingleton<PropertyRepository>();
builder.Services.AddSingleton<PropertyService>();
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

adminGroup.MapGet("/{id}", GetPropertyById)
    .WithName("GetAdminPropertyById");

adminGroup.MapPost("/", CreatePropertyHandler)
    .WithName("CreateProperty");

adminGroup.MapPut("/{id}", UpdatePropertyHandler)
    .WithName("UpdateProperty");

adminGroup.MapDelete("/{id}", DeletePropertyHandler)
    .WithName("DeleteProperty");

// Public properties (from Phase 1)
var group = app.MapGroup("/api/properties").WithName("Properties");

group.MapGet("/", GetAllProperties)
    .WithName("GetAllProperties");

group.MapGet("/{id}", GetPropertyById)
    .WithName("GetPropertyById");

// Handlers
Task<IResult> LoginHandler(LoginRequest request, AuthService authService)
{
    var (success, token, message) = authService.Login(request.Email, request.Password);
    
    if (!success)
        return Task.FromResult(Results.Unauthorized());

    return Task.FromResult(Results.Ok(new AuthResponse
    {
        Success = true,
        Message = message,
        Token = token,
        ExpiresInSeconds = 86400
    }));
}

async Task<IResult> GetAllPropertiesAdmin(PropertyRepository repo)
{
    var properties = await repo.GetAllPropertiesAsync();
    return Results.Ok(properties);
}

async Task<IResult> CreatePropertyHandler(CreatePropertyRequest request, PropertyService service)
{
    // Validate input
    var validationErrors = service.ValidateProperty(request);
    if (validationErrors.Count > 0)
    {
        return Results.BadRequest(new ValidationErrorResponse { Errors = validationErrors });
    }

    // Insert property
    var (success, property, error) = await service.InsertPropertyAsync(request);
    if (!success)
    {
        return Results.Json(new { message = error ?? "Failed to create property" }, statusCode: 500);
    }

    return Results.Created($"/api/admin/properties/{property?.Id}", property);
}

async Task<IResult> UpdatePropertyHandler(int id, UpdatePropertyRequest request, PropertyService service)
{
    // Create a validation request from update request (same fields)
    var validationRequest = new CreatePropertyRequest
    {
        Name = request.Name,
        Area = request.Area,
        MonthlyRent = request.MonthlyRent,
        ShortDesc = request.ShortDesc,
        Description = request.Description,
        Features = request.Features,
        CoverImage = request.CoverImage,
        Images = request.Images
    };

    var validationErrors = service.ValidateProperty(validationRequest);
    if (validationErrors.Count > 0)
    {
        return Results.BadRequest(new ValidationErrorResponse { Errors = validationErrors });
    }

    var (success, property, error) = await service.UpdatePropertyAsync(id, request);
    if (!success)
    {
        if (error?.Contains("not found") == true)
            return Results.NotFound(new { message = error });
        return Results.Json(new { message = error ?? "Failed to update property" }, statusCode: 500);
    }

    return Results.Ok(property);
}

async Task<IResult> DeletePropertyHandler(int id, PropertyService service)
{
    var (success, error) = await service.DeletePropertyAsync(id);
    if (!success)
    {
        if (error?.Contains("not found") == true)
            return Results.NotFound(new { message = error });
        return Results.Json(new { message = error ?? "Failed to delete property" }, statusCode: 500);
    }

    return Results.NoContent();
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
