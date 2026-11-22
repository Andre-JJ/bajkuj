using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Bajkuj.Models;

public class BikeRental
{
    public long Id { get; set; }
    [MaxLength(255)]
    public required string UserId { get; set; }
    public int BikeId { get; set; }
    public int? BikeUnitId { get; set; }
    [MaxLength(128)]
    public required string BikeModel { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly ReturnDate { get; set; }
    [Precision(10, 2)]
    public decimal DailyRate { get; set; }
    [Precision(12, 2)]
    public decimal? TotalPrice { get; set; }
    public RentalTerm Term { get; set; }
    public DateOnly? CancelledOn { get; set; }
    [MaxLength(255)]
    public string? CheckoutSessionId { get; set; }
    public bool PaymentPending { get; set; }
    public bool IsTestRental { get; set; }
    public List<BikeRentalDay> Days { get; set; } = [];

    public bool IsActive(DateOnly date) => !PaymentPending && CancelledOn is null && StartDate <= date && ReturnDate > date;
    public decimal Price => TotalPrice ?? DailyRate * (ReturnDate.DayNumber - StartDate.DayNumber);
}

[PrimaryKey(nameof(BikeId), nameof(Date))]
public class BikeRentalDay
{
    public int BikeId { get; set; }
    public DateOnly Date { get; set; }
    public long BikeRentalId { get; set; }
}

public enum RentalTerm
{
    LegacyDaily,
    Monthly,
    Yearly
}

public record BikeRentalForm(Bicykel Bike, DateOnly StartDate, RentalTerm Term)
{
    public DateOnly ReturnDate => Term == RentalTerm.Yearly ? StartDate.AddYears(1) : StartDate.AddMonths(1);
    public decimal Price => Term == RentalTerm.Yearly ? Bike.CenaZaRok : Bike.CenaZaMesiac;
}
