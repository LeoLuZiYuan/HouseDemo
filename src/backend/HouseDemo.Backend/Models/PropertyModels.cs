namespace HouseDemo.Backend.Models;

/// <summary>
/// DTO for creating a new property via POST /api/admin/properties
/// </summary>
public class CreatePropertyRequest
{
    public string? Name { get; set; }
    public decimal Area { get; set; }
    public decimal MonthlyRent { get; set; }
    public string? ShortDesc { get; set; }
    public string? Description { get; set; }
    public List<string>? Features { get; set; }
    public string? CoverImage { get; set; }
    public List<string>? Images { get; set; }
}

/// <summary>
/// DTO for updating an existing property via PUT /api/admin/properties/{id}
/// </summary>
public class UpdatePropertyRequest
{
    public string? Name { get; set; }
    public decimal Area { get; set; }
    public decimal MonthlyRent { get; set; }
    public string? ShortDesc { get; set; }
    public string? Description { get; set; }
    public List<string>? Features { get; set; }
    public string? CoverImage { get; set; }
    public List<string>? Images { get; set; }
}

/// <summary>
/// DTO for property responses (full property object with timestamps)
/// </summary>
public class PropertyResponse
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Area { get; set; }
    public string? MonthlyRent { get; set; }
    public string? ShortDesc { get; set; }
    public string? Description { get; set; }
    public List<string>? Features { get; set; }
    public string? CoverImage { get; set; }
    public List<string>? Images { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// Validation error response
/// </summary>
public class ValidationErrorResponse
{
    public List<string>? Errors { get; set; }
}
