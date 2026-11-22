namespace Bajkuj.Models;

public class Zmluva
{
    public int Id { get; set; }
    public int PrenajomId { get; set; }
    public Prenajom Prenajom { get; set; } = null!;
    public string? CisloZmluvy { get; set; }
    public DateTime? DatumVytvorenia { get; set; }
    public DateTime? DatumPodpisu { get; set; }
    public string? Stav { get; set; }
    public string? DokumentUrl { get; set; }
}