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
    public DbSet<Prenajom> Prenajmy => Set<Prenajom>();
    public DbSet<Zmluva> Zmluvy => Set<Zmluva>();
    public DbSet<Platba> Platby => Set<Platba>();
    public DbSet<Servis> Servisy => Set<Servis>();

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

        builder.Entity<LegacyBicycle>(bike =>
        {
            bike.Property(x => x.CenaZaDen).HasPrecision(10, 2);
            bike.ToTable("LegacyBicykle", table => table.HasCheckConstraint("CK_Bicykel_Cena", "CenaZaDen >= 0"));
        });
        builder.Entity<Prenajom>(rental =>
        {
            rental.Property(x => x.CelkovaCena).HasPrecision(10, 2);
            rental.ToTable(table => table.HasCheckConstraint("CK_Prenajom_Datum", "DatumDo IS NULL OR DatumDo > DatumOd"));
            rental.HasOne(x => x.Bicykel).WithMany().HasForeignKey(x => x.BicykelId)
                .HasConstraintName("FK_Prenajmy_Bicykle_BicykelId");
        });
        builder.Entity<Zmluva>(agreement =>
        {
            agreement.HasIndex(x => x.CisloZmluvy).IsUnique();
            agreement.HasOne(x => x.Prenajom).WithOne(x => x.Zmluva).HasForeignKey<Zmluva>(x => x.PrenajomId);
        });
        builder.Entity<Platba>().Property(x => x.Suma).HasPrecision(10, 2);
        builder.Entity<Servis>(service =>
        {
            service.Property(x => x.Naklady).HasPrecision(10, 2);
            service.HasOne(x => x.Bicykel).WithMany().HasForeignKey(x => x.BicykelId)
                .HasConstraintName("FK_Servisy_Bicykle_BicykelId");
        });
    }
}
