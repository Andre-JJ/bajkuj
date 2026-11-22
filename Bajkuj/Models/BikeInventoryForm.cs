namespace Bajkuj.Models;

public class BikeInventoryForm
{
    public string? Name { get; set; }
    public string? Type { get; set; }
    public string? Status { get; set; } = BikeUnit.Available;
    public int? Units { get; set; } = 1;
    public string? MonthlyPrice { get; set; }
    public string? YearlyPrice { get; set; }
    public IFormFile? Photo { get; set; }
}
