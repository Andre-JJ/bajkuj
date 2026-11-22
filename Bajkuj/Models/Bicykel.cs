using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Bajkuj.Models;

public class Bicykel
{
    public int Id { get; set; }
    [MaxLength(128)] public string Model { get; set; } = "";
    [MaxLength(40)] public string Typ { get; set; } = "";
    [Precision(10, 2)] public decimal CenaZaMesiac { get; set; }
    [Precision(10, 2)] public decimal CenaZaRok { get; set; }
    public DateTime DatumPridania { get; set; }
    public bool HasCustomPhoto { get; set; }
    public List<BikeUnit> Units { get; set; } = [];
    public BikePhoto? CustomPhoto { get; set; }

    public string? PhotoPath => HasCustomPhoto ? $"~/bicykle/{Id}/fotografia" : Id switch
    {
        101 or 104 or 105 or 106 => $"~/prototype/bikes/{Id}.jpg",
        102 or 103 => $"~/prototype/bikes/{Id}.png",
        _ => null
    };
}

public class BikePhoto
{
    [Key] public int BicykelId { get; set; }
    [MaxLength(20)] public required string ContentType { get; set; }
    [Column(TypeName = "mediumblob")] public required byte[] Content { get; set; }
}

public class BikeUnit
{
    public const string Available = "dostupny";
    public const string Rented = "prenajaty";
    public const string InService = "v_servise";

    public int Id { get; set; }
    public int BicykelId { get; set; }
    [MaxLength(20)] public string Stav { get; set; } = Available;
}

public record BikeStock(Bicykel Bike, int AvailableUnits, int TotalUnits, int ServiceUnits)
{
    public string Status => AvailableUnits > 0 ? BikeUnit.Available
        : ServiceUnits == TotalUnits && TotalUnits > 0 ? BikeUnit.InService : BikeUnit.Rented;
}
