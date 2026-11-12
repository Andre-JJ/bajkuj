namespace Bajkuj.Models;

public class Platba
{
    public int Id { get; set; }
    public int PrenajomId { get; set; }
    public Prenajom Prenajom { get; set; } = null!;
    public decimal Suma { get; set; }
    public DateTime? DatumPlatby { get; set; }
    public string? SposobPlatby { get; set; }
    public string? Stav { get; set; }
}