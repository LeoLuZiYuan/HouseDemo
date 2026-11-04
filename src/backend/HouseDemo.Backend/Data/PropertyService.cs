using Dapper;
using HouseDemo.Backend.Models;
using System.Data;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace HouseDemo.Backend.Data;

public class PropertyService
{
    private readonly Database _db;

    public PropertyService(Database db) => _db = db;

    /// <summary>
    /// Validate property data before insert/update
    /// Returns list of validation errors; empty list means valid
    /// </summary>
    public List<string> ValidateProperty(CreatePropertyRequest request)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(request.Name))
            errors.Add("Property name is required");

        if (request.Area <= 0)
            errors.Add("Area must be a positive number");

        if (request.MonthlyRent <= 0)
            errors.Add("Monthly rent must be a positive number");

        if (string.IsNullOrWhiteSpace(request.ShortDesc))
            errors.Add("Short description is required");

        if (string.IsNullOrWhiteSpace(request.Description))
            errors.Add("Full description is required");

        if (string.IsNullOrWhiteSpace(request.CoverImage))
            errors.Add("Cover image URL is required");
        else if (!IsValidUrl(request.CoverImage))
            errors.Add("Cover image must be a valid URL");

        // Validate images if provided
        if (request.Images != null && request.Images.Count > 0)
        {
            foreach (var url in request.Images)
            {
                if (!IsValidUrl(url))
                {
                    errors.Add($"Invalid image URL: {url}");
                    break;
                }
            }
        }

        return errors;
    }

    /// <summary>
    /// Insert a new property into the database
    /// Returns the newly inserted property with generated ID
    /// </summary>
    public async Task<(bool Success, PropertyResponse? Property, string? Error)> InsertPropertyAsync(CreatePropertyRequest request)
    {
        try
        {
            using var conn = _db.GetConnection();
            conn.Open();

            // Convert comma-separated strings to JSON arrays for storage
            var featuresJson = ConvertToJsonArray(request.Features);
            var imagesJson = ConvertToJsonArray(request.Images);

            var now = DateTime.UtcNow;

            var sql = @"
                INSERT INTO properties (name, area, monthly_rent, short_desc, description, features, cover_image, images, created_at, updated_at)
                VALUES (@Name, @Area, @MonthlyRent, @ShortDesc, @Description, @Features::jsonb, @CoverImage, @Images::jsonb, @CreatedAt, @UpdatedAt)
                RETURNING id, name, area, monthly_rent, short_desc, description, features, cover_image, images, created_at, updated_at";

            var result = await conn.QueryFirstOrDefaultAsync<PropertyDto>(sql, new
            {
                Name = request.Name,
                Area = request.Area,
                MonthlyRent = request.MonthlyRent,
                ShortDesc = request.ShortDesc,
                Description = request.Description,
                Features = featuresJson,
                CoverImage = request.CoverImage,
                Images = imagesJson,
                CreatedAt = now,
                UpdatedAt = now
            });

            if (result == null)
                return (false, null, "Failed to insert property");

            var response = MapToPropertyResponse(result);
            return (true, response, null);
        }
        catch (Exception ex)
        {
            return (false, null, $"Database error: {ex.Message}");
        }
    }

    /// <summary>
    /// Update an existing property
    /// </summary>
    public async Task<(bool Success, PropertyResponse? Property, string? Error)> UpdatePropertyAsync(int id, UpdatePropertyRequest request)
    {
        try
        {
            using var conn = _db.GetConnection();
            conn.Open();

            // First check if property exists
            var existing = await conn.QueryFirstOrDefaultAsync<PropertyDto>(
                "SELECT id FROM properties WHERE id = @id",
                new { id }
            );

            if (existing == null)
                return (false, null, "Property not found");

            var featuresJson = ConvertToJsonArray(request.Features);
            var imagesJson = ConvertToJsonArray(request.Images);
            var now = DateTime.UtcNow;

            var sql = @"
                UPDATE properties 
                SET name = @Name, area = @Area, monthly_rent = @MonthlyRent, 
                    short_desc = @ShortDesc, description = @Description, 
                    features = @Features::jsonb, cover_image = @CoverImage, images = @Images::jsonb, updated_at = @UpdatedAt
                WHERE id = @Id
                RETURNING id, name, area, monthly_rent, short_desc, description, features, cover_image, images, created_at, updated_at";

            var result = await conn.QueryFirstOrDefaultAsync<PropertyDto>(sql, new
            {
                Id = id,
                Name = request.Name,
                Area = request.Area,
                MonthlyRent = request.MonthlyRent,
                ShortDesc = request.ShortDesc,
                Description = request.Description,
                Features = featuresJson,
                CoverImage = request.CoverImage,
                Images = imagesJson,
                UpdatedAt = now
            });

            if (result == null)
                return (false, null, "Failed to update property");

            var response = MapToPropertyResponse(result);
            return (true, response, null);
        }
        catch (Exception ex)
        {
            return (false, null, $"Database error: {ex.Message}");
        }
    }

    /// <summary>
    /// Delete a property by ID
    /// </summary>
    public async Task<(bool Success, string? Error)> DeletePropertyAsync(int id)
    {
        try
        {
            using var conn = _db.GetConnection();
            conn.Open();

            var result = await conn.ExecuteAsync(
                "DELETE FROM properties WHERE id = @id",
                new { id }
            );

            if (result == 0)
                return (false, "Property not found");

            return (true, null);
        }
        catch (Exception ex)
        {
            return (false, $"Database error: {ex.Message}");
        }
    }

    // ==================== Helper Methods ====================

    private static bool IsValidUrl(string url)
    {
        try
        {
            var uri = new Uri(url);
            return uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps;
        }
        catch
        {
            return false;
        }
    }

    private static List<string> ParseCommaSeparatedList(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return new List<string>();

        return input
            .Split(',')
            .Select(s => s.Trim())
            .Where(s => !string.IsNullOrEmpty(s))
            .ToList();
    }

    private static string ConvertToJsonArray(List<string>? input)
    {
        if (input == null || input.Count == 0)
            return "[]";

        return JsonSerializer.Serialize(input);
    }

    private static PropertyResponse MapToPropertyResponse(PropertyDto dto)
    {
        return new PropertyResponse
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
