using HouseDemo.Backend.Data;
using HouseDemo.Backend.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<Database>();
builder.Services.AddSingleton<PropertyRepository>();

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

app.MapGet("/api/properties", GetAllProperties);
app.MapGet("/api/properties/{id}", GetPropertyById);

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
