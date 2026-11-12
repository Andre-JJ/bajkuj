namespace Bajkuj.Models;

public class Bicykel
{
    public int Id { get; set; }
    public string Model { get; set; } = "";
    public string Typ { get; set; } = "";
    public string Stav { get; set; } = "dostupny";
    public decimal CenaZaDen { get; set; }
    public DateTime DatumPridania { get; set; }
}