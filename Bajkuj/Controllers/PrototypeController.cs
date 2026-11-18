using Bajkuj.Models;
using Microsoft.AspNetCore.Mvc;

namespace Bajkuj.Controllers;

public class PrototypeController : Controller
{
    private static readonly IReadOnlyList<Bicykel> MockBicykle =
    [
        new() { Id = 101, Model = "Kellys Avenue 30", Typ = "Mestský", Stav = "dostupny", CenaZaDen = 8m, DatumPridania = new DateTime(2026, 9, 15) },
        new() { Id = 102, Model = "CTM Rambler 2.0", Typ = "Horský", Stav = "prenajaty", CenaZaDen = 12.50m, DatumPridania = new DateTime(2026, 9, 28) },
        new() { Id = 103, Model = "Cube Touring Hybrid ONE", Typ = "Elektrobicykel", Stav = "v_servise", CenaZaDen = 22m, DatumPridania = new DateTime(2026, 10, 6) },
        new() { Id = 104, Model = "CTM Bora 2.0", Typ = "Mestský", Stav = "dostupny", CenaZaDen = 9m, DatumPridania = new DateTime(2026, 10, 12) },
        new() { Id = 105, Model = "Giant Escape 3", Typ = "Trekingový", Stav = "dostupny", CenaZaDen = 11m, DatumPridania = new DateTime(2026, 10, 14) },
        new() { Id = 106, Model = "Merida Big Nine 20", Typ = "Horský", Stav = "dostupny", CenaZaDen = 13m, DatumPridania = new DateTime(2026, 10, 19) }
    ];

    [HttpGet("/prototype/{role}/{section}", Name = "prototype")]
    public IActionResult Preview(string role, string section)
    {
        var navigation = role switch
        {
            "admin" => PrototypeNavigation.Admin,
            "user" => PrototypeNavigation.User,
            _ => null
        };

        if (navigation is null || !navigation.Any(item => item.Section == section))
        {
            return NotFound();
        }

        ViewData["PrototypeRole"] = role;
        ViewData["PrototypeSection"] = section;

        if (section == "bicykle")
        {
            return View("~/Views/Shared/_PrototypeBicykle.cshtml", MockBicykle);
        }

        var folder = role == "admin" ? "Admin" : "User";
        return View($"~/Views/{folder}/{section}.cshtml");
    }
}
