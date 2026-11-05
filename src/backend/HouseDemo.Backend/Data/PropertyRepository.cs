using Dapper;
using HouseDemo.Backend.Models;
using System.Data;
using System.Text.Json;

namespace HouseDemo.Backend.Data;

public class PropertyRepository
{
    private readonly Database _db;

    public PropertyRepository(Database db) => _db = db;

    public async Task<List<Property>> GetAllPropertiesAsync()
    {
        using var conn = _db.GetConnection();
        conn.Open();
        
        var properties = await conn.QueryAsync<PropertyDto>(
            "SELECT id, name, area, monthly_rent, short_desc, description, features, cover_image, images, created_at, updated_at FROM properties ORDER BY id"
        );
        
        return properties.Select(MapToProperty).ToList();
    }

    public async Task<Property?> GetPropertyByIdAsync(int id)
    {
        using var conn = _db.GetConnection();
        conn.Open();
        
        var result = await conn.QueryFirstOrDefaultAsync<PropertyDto>(
            "SELECT id, name, area, monthly_rent, short_desc, description, features, cover_image, images, created_at, updated_at FROM properties WHERE id = @id",
            new { id }
        );
        
        return result != null ? MapToProperty(result) : null;
    }

    private static Property MapToProperty(PropertyDto dto)
    {
        return new Property
        {
            Id = dto.id,
            Name = dto.name,
            Area = dto.area,
            MonthlyRent = dto.monthly_rent,
            ShortDesc = dto.short_desc,
            Description = dto.description,
            Features = ParseJsonArray(dto.features),
            CoverImage = dto.cover_image,
            Images = ParseJsonArray(dto.images),
            CreatedAt = dto.created_at,
            UpdatedAt = dto.updated_at
        };
    }

    private static List<string>? ParseJsonArray(string? json)
    {
        if (string.IsNullOrEmpty(json)) return null;
        try
        {
            return JsonSerializer.Deserialize<List<string>>(json);
        }
        catch
        {
            return null;
        }
    }

    private class PropertyDto
    {
        public int id { get; set; }
        public string? name { get; set; }
        public string? area { get; set; }
        public string? monthly_rent { get; set; }
        public string? short_desc { get; set; }
        public string? description { get; set; }
        public string? features { get; set; }
        public string? cover_image { get; set; }
        public string? images { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
    }
}
