namespace Bajkuj.Models;

public class Servis
{
    public int Id { get; set; }
    public int BicykelId { get; set; }
    public Bicykel Bicykel { get; set; } = null!;
    public DateTime? DatumServisu { get; set; }
    public string? Popis { get; set; }
    public decimal? Naklady { get; set; }
    public string? Stav { get; set; }
}