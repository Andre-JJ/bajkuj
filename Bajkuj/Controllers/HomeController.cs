using System.Diagnostics;
using Bajkuj.Data;
using Microsoft.AspNetCore.Mvc;
using Bajkuj.Models;
using Bajkuj.Localization;
using Microsoft.AspNetCore.Localization;

namespace Bajkuj.Controllers;

public class HomeController : Controller
{
    [HttpPost("/language")]
    [ValidateAntiForgeryToken]
    public IActionResult SetLanguage(string culture, string returnUrl)
    {
        if (culture is not (AppText.Slovak or AppText.English))
            return BadRequest();

        Response.Cookies.Append(CookieRequestCultureProvider.DefaultCookieName,
            CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
            new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1), IsEssential = true,
                HttpOnly = true, SameSite = SameSiteMode.Lax, Secure = Request.IsHttps });

        return Url.IsLocalUrl(returnUrl) ? LocalRedirect(returnUrl) : RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Index([FromServices] ApplicationDbContext db, CancellationToken cancellationToken)
    {
        var stock = await BikeCatalogue.GetStockAsync(db, BikeCatalogue.Today, cancellationToken);
        var available = stock.Where(item => item.AvailableUnits > 0).ToList();
        return View(available);
    }

    [HttpGet("/prenajom", Name = "start-rental")]
    public async Task<IActionResult> StartRental([FromQuery] int bikeId, [FromServices] ApplicationDbContext db,
        CancellationToken cancellationToken)
    {
        var stock = await BikeCatalogue.FindStockAsync(db, bikeId, BikeCatalogue.Today, cancellationToken);
        if (stock is null || stock.AvailableUnits == 0)
        {
            TempData["LandingRentalNotice"] = "Tento bicykel už nie je dostupný.";
            return RedirectToAction(nameof(Index), "Home", null, "dostupne-bicykle");
        }

        var rentalUrl = Url.RouteUrl("rent-bike", new { id = stock.Bike.Id })!;
        return User.Identity?.IsAuthenticated == true
            ? LocalRedirect(rentalUrl)
            : RedirectToPage("/Account/Register", new { area = "Identity", returnUrl = rentalUrl });
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
