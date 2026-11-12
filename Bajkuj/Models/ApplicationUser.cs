using Microsoft.AspNetCore.Identity;

namespace Bajkuj.Models;

public class ApplicationUser : IdentityUser
{
    public string Meno { get; set; } = "";
    public string Priezvisko { get; set; } = "";
    public ICollection<Prenajom> Prenajmy { get; set; } = new List<Prenajom>();
}