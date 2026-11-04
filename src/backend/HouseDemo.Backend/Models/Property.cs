namespace HouseDemo.Backend.Models;

public class Property
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
