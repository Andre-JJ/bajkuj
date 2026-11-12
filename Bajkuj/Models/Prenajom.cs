namespace Bajkuj.Models;

public class Prenajom
{
    public int Id { get; set; }
    public string UserId { get; set; } = "";
    public ApplicationUser User { get; set; } = null!;
    public int BicykelId { get; set; }
    public Bicykel Bicykel { get; set; } = null!;
    public DateTime DatumOd { get; set; }
    public DateTime? DatumDo { get; set; }
    public decimal? CelkovaCena { get; set; }
    public string Stav { get; set; } = "";
    public Zmluva? Zmluva { get; set; }
    public ICollection<Platba> Platby { get; set; } = new List<Platba>();
}