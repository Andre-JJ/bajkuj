using System.Data;
using System.Net;
using System.Security.Claims;
using Bajkuj.Data;
using Bajkuj.Localization;
using Bajkuj.Models;
using Bajkuj.Payments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;

namespace Bajkuj.Controllers;

public class PrototypeController(StripeCheckout checkout, IWebHostEnvironment environment, IConfiguration configuration,
    ILogger<PrototypeController> logger) : Controller
{
    private const int MaxActiveRentals = 3;

    [HttpGet("/prototype/{role}/{section}", Name = "prototype")]
    public async Task<IActionResult> Preview(string role, string section, CancellationToken cancellationToken)
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

        if (role == "admin" && !User.IsInRole(AdminAccounts.Role))
            return User.Identity?.IsAuthenticated == true ? Forbid() : Challenge();

        ViewData["PrototypeRole"] = role;
        ViewData["PrototypeSection"] = section;

        if (section == "bicykle")
        {
            var db = HttpContext.RequestServices.GetRequiredService<ApplicationDbContext>();
            return View("~/Views/Shared/_PrototypeBicykle.cshtml",
                await BikeCatalogue.GetStockAsync(db, BikeCatalogue.Today, cancellationToken));
        }

        if (role == "user" && section == "index" && User.FindFirstValue(ClaimTypes.NameIdentifier) is string userId)
        {
            var db = HttpContext.RequestServices.GetRequiredService<ApplicationDbContext>();
            ViewData["ActiveRentals"] = await ActiveRentals(db, userId, BikeCatalogue.Today)
                .CountAsync(rental => !rental.PaymentPending, cancellationToken);
        }

        var folder = role == "admin" ? "Admin" : "User";
        return View($"~/Views/{folder}/{section}.cshtml");
    }

    [Authorize]
    [HttpGet("/bicykle/{id:int}/prenajat", Name = "rent-bike")]
    public async Task<IActionResult> Rent([FromRoute] int id, [FromServices] ApplicationDbContext db,
        CancellationToken cancellationToken)
    {
        var today = BikeCatalogue.Today;
        var stock = await BikeCatalogue.FindStockAsync(db, id, today, cancellationToken);
        if (stock is null)
            return NotFound();
        if (stock.AvailableUnits == 0)
            return BikeUnavailable();
        if (User.FindFirstValue(ClaimTypes.NameIdentifier) is not string userId)
            return Challenge();
        return RentalForm(new BikeRentalForm(stock.Bike, today, RentalTerm.Monthly));
    }

    [Authorize]
    [HttpPost("/bicykle/{id:int}/prenajat")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Rent([FromRoute] int id, [FromForm] RentalTerm term, [FromForm] DateOnly startDate,
        [FromForm] bool testCheckout,
        [FromServices] ApplicationDbContext db,
        CancellationToken cancellationToken)
    {
        if (User.FindFirstValue(ClaimTypes.NameIdentifier) is not string userId)
            return Challenge();

        if (testCheckout ? !CanUseTestCheckout() : !checkout.IsConfigured)
        {
            TempData["RentalNotice"] = "Platba kartou momentálne nie je dostupná.";
            return RedirectToRoute("rent-bike", new { id });
        }

        // Serialize bookings for the same account and assignments for the same model.
        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
        if (!await LockUserAsync(db, userId, cancellationToken))
            return Challenge();
        var bike = await BikeCatalogue.LockModelAsync(db, id, cancellationToken);
        if (bike is null)
            return NotFound();

        var today = BikeCatalogue.Today;
        if (!ModelState.IsValid || term is not (RentalTerm.Monthly or RentalTerm.Yearly))
        {
            ModelState.Clear();
            ModelState.AddModelError("", AppText.T("Vyberte mesačný alebo ročný prenájom."));
            return RentalForm(new BikeRentalForm(bike, today, RentalTerm.Monthly));
        }
        var form = new BikeRentalForm(bike, today, term);
        if (startDate != today)
        {
            ModelState.AddModelError("", AppText.T("Dátum začiatku sa zmenil. Skontrolujte termín a potvrďte prenájom znova."));
            return RentalForm(form);
        }

        if (!testCheckout)
        {
            var pending = await ActiveRentals(db, userId, today)
                .Where(rental => rental.PaymentPending && rental.Term == term && rental.StartDate == today &&
                    db.BikeUnits.Any(unit => unit.Id == rental.BikeId && unit.BicykelId == id))
                .Select(rental => (long?)rental.Id).FirstOrDefaultAsync(cancellationToken);
            if (pending is not null)
                return RedirectToRoute("rental-payment", new { id = pending.Value });
        }
        if (await ActiveRentals(db, userId, today).CountAsync(cancellationToken) >= MaxActiveRentals)
            return RentalLimitReached();
        var unitId = await BikeCatalogue.FindFreeUnitAsync(db, id, today, form.ReturnDate, cancellationToken);
        if (unitId is null)
            return BikeUnavailable();

        var rental = new BikeRental
        {
            UserId = userId,
            BikeId = unitId.Value,
            BikeUnitId = unitId.Value,
            BikeModel = bike.Model,
            StartDate = today,
            ReturnDate = form.ReturnDate,
            TotalPrice = form.Price,
            Term = term,
            PaymentPending = !testCheckout,
            IsTestRental = testCheckout,
            Days = Enumerable.Range(0, form.ReturnDate.DayNumber - today.DayNumber).Select(offset => new BikeRentalDay
            {
                BikeId = unitId.Value, Date = today.AddDays(offset)
            }).ToList()
        };
        db.BikeRentals.Add(rental);
        try
        {
            await db.SaveChangesAsync(cancellationToken);
            if (!testCheckout)
            {
                var returnPath = Url.RouteUrl("rental-payment", new { id = rental.Id })!;
                var session = await checkout.CreateAsync(rental, returnPath, AppText.Language, cancellationToken);
                if (string.IsNullOrWhiteSpace(session.Id) || string.IsNullOrWhiteSpace(session.ClientSecret))
                    throw new HttpRequestException("Checkout did not return a usable session.");
                rental.CheckoutSessionId = session.Id;
                await db.SaveChangesAsync(cancellationToken);
            }
            await transaction.CommitAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (
            exception.InnerException is MySqlException { ErrorCode: MySqlErrorCode.DuplicateKeyEntry })
        {
            return BikeUnavailable();
        }
        catch (Exception exception) when (exception is HttpRequestException or System.Text.Json.JsonException
            || exception is OperationCanceledException && !cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning("Stripe checkout could not be started for rental {RentalId}.", rental.Id);
            TempData["RentalNotice"] = "Platbu sa nepodarilo spustiť. Skúste znova.";
            return RedirectToRoute("rent-bike", new { id });
        }

        if (!testCheckout)
            return RedirectToRoute("rental-payment", new { id = rental.Id });

        TempData["RentalConfirmation"] = "Testovací prenájom bol potvrdený.";
        return RedirectToRoute("my-rentals");
    }

    [Authorize]
    [HttpGet("/moje-prenajmy", Name = "my-rentals")]
    public async Task<IActionResult> MyRentals([FromServices] ApplicationDbContext db, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null)
            return Challenge();

        ViewData["PrototypeRole"] = "user";
        ViewData["PrototypeSection"] = "bicykle";
        ViewData["RentalToday"] = BikeCatalogue.Today;
        var rentals = await db.BikeRentals.AsNoTracking().Where(rental => rental.UserId == userId)
            .OrderByDescending(rental => rental.StartDate).ThenByDescending(rental => rental.Id)
            .ToListAsync(cancellationToken);
        return View("~/Views/User/moje-prenajmy.cshtml", rentals);
    }

    [Authorize]
    [HttpPost("/moje-prenajmy/{id:long}/zrusit", Name = "cancel-rental")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelRental([FromRoute] long id, [FromServices] ApplicationDbContext db,
        CancellationToken cancellationToken)
    {
        if (User.FindFirstValue(ClaimTypes.NameIdentifier) is not string userId)
            return Challenge();

        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
        if (!await LockUserAsync(db, userId, cancellationToken))
            return Challenge();

        var rental = await db.BikeRentals.SingleOrDefaultAsync(item => item.Id == id && item.UserId == userId, cancellationToken);
        if (rental is null)
            return NotFound();

        var today = BikeCatalogue.Today;
        if (rental.IsActive(today))
        {
            rental.CancelledOn = today;
            await db.BikeRentalDays.Where(day => day.BikeRentalId == id && day.Date >= today)
                .ExecuteDeleteAsync(cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            TempData["RentalConfirmation"] = "Prenájom bol zrušený.";
        }

        return RedirectToRoute("my-rentals");
    }

    private static IQueryable<BikeRental> ActiveRentals(ApplicationDbContext db, string userId, DateOnly today) =>
        db.BikeRentals.Where(rental => rental.UserId == userId && rental.CancelledOn == null &&
            rental.StartDate <= today && rental.ReturnDate > today);

    private static async Task<bool> LockUserAsync(ApplicationDbContext db, string userId, CancellationToken cancellationToken) =>
        (await db.Database.SqlQuery<string>($"SELECT `Id` AS `Value` FROM `AspNetUsers` WHERE `Id` = {userId} FOR UPDATE")
            .ToListAsync(cancellationToken)).Count > 0;

    private IActionResult BikeUnavailable()
    {
        TempData["RentalNotice"] = "Tento bicykel už nie je dostupný.";
        return RedirectToRoute("prototype", new { role = "user", section = "bicykle" });
    }

    private IActionResult RentalLimitReached()
    {
        TempData["RentalConfirmation"] = "Naraz môžete mať prenajaté najviac 3 bicykle.";
        return RedirectToRoute("my-rentals");
    }

    private ViewResult RentalForm(BikeRentalForm form)
    {
        ViewData["PrototypeRole"] = "user";
        ViewData["PrototypeSection"] = "bicykle";
        ViewData["CardPaymentsAvailable"] = checkout.IsConfigured;
        ViewData["LocalTestCheckoutAvailable"] = CanUseTestCheckout();
        return View("~/Views/User/prenajat.cshtml", form);
    }

    private bool CanUseTestCheckout() => !checkout.IsConfigured && environment.IsDevelopment() &&
        configuration.GetValue<bool>("RentalPayments:EnableLocalTestCheckout") &&
        HttpContext.Connection.RemoteIpAddress is IPAddress address && IPAddress.IsLoopback(address) &&
        (Request.Host.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase) ||
            IPAddress.TryParse(Request.Host.Host, out var hostAddress) && IPAddress.IsLoopback(hostAddress));
}
