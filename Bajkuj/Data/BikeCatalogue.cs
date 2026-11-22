using Bajkuj.Models;
using Microsoft.EntityFrameworkCore;

namespace Bajkuj.Data;

public static class BikeCatalogue
{
    private static readonly TimeZoneInfo RentalTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Bratislava");

    public static DateOnly Today => DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, RentalTimeZone).DateTime);

    public static Task<List<BikeStock>> GetStockAsync(ApplicationDbContext db, DateOnly date, CancellationToken cancellationToken) =>
        Stock(db, db.Bicykle.OrderBy(bike => bike.Id), date).ToListAsync(cancellationToken);

    public static Task<BikeStock?> FindStockAsync(ApplicationDbContext db, int id, DateOnly date,
        CancellationToken cancellationToken) =>
        Stock(db, db.Bicykle.Where(bike => bike.Id == id), date).FirstOrDefaultAsync(cancellationToken);

    private static IQueryable<BikeStock> Stock(ApplicationDbContext db, IQueryable<Bicykel> bikes, DateOnly date) =>
        bikes.AsNoTracking().Select(bike => new BikeStock(
            bike,
            bike.Units.Count(unit => unit.Stav == BikeUnit.Available &&
                !db.BikeRentalDays.Any(day => day.BikeId == unit.Id && day.Date == date)),
            bike.Units.Count,
            bike.Units.Count(unit => unit.Stav == BikeUnit.InService)));

    public static Task<int?> FindFreeUnitAsync(ApplicationDbContext db, int modelId, DateOnly start, DateOnly end,
        CancellationToken cancellationToken) =>
        db.BikeUnits.Where(unit => unit.BicykelId == modelId && unit.Stav == BikeUnit.Available &&
                !db.BikeRentalDays.Any(day => day.BikeId == unit.Id && day.Date >= start && day.Date < end))
            .OrderBy(unit => unit.Id).Select(unit => (int?)unit.Id).FirstOrDefaultAsync(cancellationToken);

    public static async Task<Bicykel?> LockModelAsync(ApplicationDbContext db, int id, CancellationToken cancellationToken) =>
        (await db.Bicykle.FromSqlInterpolated($"SELECT * FROM `Bicykle` WHERE `Id` = {id} FOR UPDATE")
            .ToListAsync(cancellationToken)).SingleOrDefault();

    public static void Seed(ModelBuilder builder)
    {
        builder.Entity<Bicykel>().HasData(
            new Bicykel { Id = 101, Model = "Kellys Avenue 30", Typ = "Mestský", CenaZaMesiac = 8m, CenaZaRok = 27m, DatumPridania = new(2026, 9, 15) },
            new Bicykel { Id = 102, Model = "CTM Rambler 2.0", Typ = "Horský", CenaZaMesiac = 12.50m, CenaZaRok = 31m, DatumPridania = new(2026, 9, 28) },
            new Bicykel { Id = 103, Model = "Cube Touring Hybrid ONE", Typ = "Elektrobicykel", CenaZaMesiac = 22m, CenaZaRok = 36m, DatumPridania = new(2026, 10, 6) },
            new Bicykel { Id = 104, Model = "CTM Bora 2.0", Typ = "Mestský", CenaZaMesiac = 9m, CenaZaRok = 29m, DatumPridania = new(2026, 10, 12) },
            new Bicykel { Id = 105, Model = "Giant Escape 3", Typ = "Trekingový", CenaZaMesiac = 11m, CenaZaRok = 32m, DatumPridania = new(2026, 10, 14) },
            new Bicykel { Id = 106, Model = "Merida Big Nine 20", Typ = "Horský", CenaZaMesiac = 13m, CenaZaRok = 33m, DatumPridania = new(2026, 10, 19) });

        builder.Entity<BikeUnit>().HasData(
            Fleet(101, 10, BikeUnit.Available)
                .Concat(Fleet(102, 5, BikeUnit.Rented))
                .Concat(Fleet(103, 4, BikeUnit.InService))
                .Concat(Fleet(104, 7, BikeUnit.Available))
                .Concat(Fleet(105, 12, BikeUnit.Available))
                .Concat(Fleet(106, 6, BikeUnit.Available)));
    }

    private static IEnumerable<BikeUnit> Fleet(int modelId, int count, string status) =>
        Enumerable.Range(0, count).Select(index => new BikeUnit
        {
            Id = index < 3 ? modelId + index * 1000 : modelId * 100 + index,
            BicykelId = modelId,
            Stav = status
        });
}
