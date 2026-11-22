using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Bajkuj.Models;

namespace Bajkuj.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Bicykel> Bicykle => Set<Bicykel>();
    public DbSet<BikeUnit> BikeUnits => Set<BikeUnit>();
    public DbSet<BikePhoto> BikePhotos => Set<BikePhoto>();
    public DbSet<BikeRental> BikeRentals => Set<BikeRental>();
    public DbSet<BikeRentalDay> BikeRentalDays => Set<BikeRentalDay>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<BikeRental>().HasIndex(rental => new { rental.UserId, rental.StartDate });
        builder.Entity<BikeRental>().HasIndex(rental => rental.CheckoutSessionId).IsUnique();
        builder.Entity<BikeRental>().HasOne<BikeUnit>().WithMany().HasForeignKey(rental => rental.BikeUnitId)
            .OnDelete(DeleteBehavior.SetNull);
        builder.Entity<BikePhoto>().HasOne<Bicykel>().WithOne(bike => bike.CustomPhoto).HasForeignKey<BikePhoto>(photo => photo.BicykelId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Entity<BikeUnit>().HasIndex(unit => new { unit.BicykelId, unit.Stav });
        BikeCatalogue.Seed(builder);
    }
}
