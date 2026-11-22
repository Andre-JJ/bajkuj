using System.Security.Claims;
using System.Text.Json;
using Bajkuj.Data;
using Bajkuj.Models;
using Bajkuj.Payments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bajkuj.Controllers;

public class RentalPaymentController(ApplicationDbContext db, StripeCheckout checkout) : Controller
{
    [Authorize]
    [HttpGet("/platba/{id:long}", Name = "rental-payment")]
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    public async Task<IActionResult> Index(long id, CancellationToken cancellationToken)
    {
        Response.Headers["Referrer-Policy"] = "no-referrer";
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null)
            return Challenge();
        var rental = await db.BikeRentals.AsNoTracking().FirstOrDefaultAsync(
            item => item.Id == id && item.UserId == userId, cancellationToken);
        if (rental?.CheckoutSessionId is not string sessionId)
            return NotFound();
        if (!rental.PaymentPending)
            return ReturnToRentals(rental.CancelledOn is null ? "Prenájom bol potvrdený." : "Platnosť platby vypršala.");

        if (!checkout.IsConfigured)
            return ReturnToRentals("Platba kartou momentálne nie je dostupná.");

        StripeSession session;
        try
        {
            session = await checkout.GetAsync(sessionId, cancellationToken);
        }
        catch (Exception exception) when (exception is HttpRequestException or JsonException
            || exception is OperationCanceledException && !cancellationToken.IsCancellationRequested)
        {
            return ReturnToRentals("Platbu sa nepodarilo načítať. Skúste znova.");
        }
        await StripeCheckout.ApplySessionAsync(db, session, cancellationToken);

        if (session.Status != "open" || string.IsNullOrEmpty(session.ClientSecret))
        {
            var resolved = await db.BikeRentals.AsNoTracking().SingleAsync(item => item.Id == id, cancellationToken);
            return ReturnToRentals(resolved.PaymentPending ? "Platba sa overuje."
                : resolved.CancelledOn is null ? "Prenájom bol potvrdený." : "Platnosť platby vypršala.");
        }

        ViewData["PrototypeRole"] = "user";
        ViewData["PrototypeSection"] = "bicykle";
        return View("~/Views/User/platba.cshtml", new CheckoutView(rental, checkout.PublishableKey!, session.ClientSecret));
    }

    [HttpPost("/platba/stripe-webhook")]
    [IgnoreAntiforgeryToken]
    [RequestSizeLimit(256 * 1024)]
    public async Task<IActionResult> StripeWebhook(CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(Request.Body);
        var payload = await reader.ReadToEndAsync(cancellationToken);
        if (!checkout.IsValidWebhook(Request.Headers["Stripe-Signature"].ToString(), payload))
            return BadRequest();

        StripeSession? stripeEventSession = null;
        try
        {
            using var message = JsonDocument.Parse(payload);
            var root = message.RootElement;
            if (root.GetProperty("type").GetString() is "checkout.session.completed" or "checkout.session.expired")
                stripeEventSession = root.GetProperty("data").GetProperty("object").Deserialize<StripeSession>();
        }
        catch (Exception exception) when (exception is JsonException or KeyNotFoundException or InvalidOperationException)
        {
            return BadRequest();
        }

        if (stripeEventSession is not null)
            await StripeCheckout.ApplySessionAsync(db, stripeEventSession, cancellationToken);

        return Ok();
    }

    private IActionResult ReturnToRentals(string message)
    {
        TempData["RentalConfirmation"] = message;
        return RedirectToRoute("my-rentals");
    }

    public sealed record CheckoutView(BikeRental Rental, string PublishableKey, string ClientSecret);
}
