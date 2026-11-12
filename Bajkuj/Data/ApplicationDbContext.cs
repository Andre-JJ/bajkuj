using Bajkuj.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Bajkuj.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Bicykel> Bicykle => Set<Bicykel>();
    public DbSet<Prenajom> Prenajmy => Set<Prenajom>();
    public DbSet<Zmluva> Zmluvy => Set<Zmluva>();
    public DbSet<Platba> Platby => Set<Platba>();
    public DbSet<Servis> Servisy => Set<Servis>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b); // required, configures the Identity tables

        b.Entity<Bicykel>(e =>
        {
            e.Property(x => x.CenaZaDen).HasPrecision(10, 2);
            e.ToTable(t => t.HasCheckConstraint("CK_Bicykel_Cena", "CenaZaDen >= 0"));
        });

        b.Entity<Prenajom>(e =>
        {
            e.Property(x => x.CelkovaCena).HasPrecision(10, 2);
            e.ToTable(t => t.HasCheckConstraint("CK_Prenajom_Datum", "DatumDo IS NULL OR DatumDo > DatumOd"));
        });

        b.Entity<Zmluva>(e =>
        {
            e.HasIndex(x => x.CisloZmluvy).IsUnique();
            e.HasOne(x => x.Prenajom).WithOne(p => p.Zmluva)
                .HasForeignKey<Zmluva>(x => x.PrenajomId);
        });

        b.Entity<Platba>().Property(x => x.Suma).HasPrecision(10, 2);
        b.Entity<Servis>().Property(x => x.Naklady).HasPrecision(10, 2);
    }
}