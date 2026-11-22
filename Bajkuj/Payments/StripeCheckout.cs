using System.Globalization;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Bajkuj.Data;
using Bajkuj.Models;
using Microsoft.EntityFrameworkCore;

namespace Bajkuj.Payments;

public sealed class StripeCheckout(HttpClient client, IConfiguration configuration)
{
    private string? Secret => configuration["Stripe:SecretKey"];
    private string? WebhookSecret => configuration["Stripe:WebhookSecret"];
    public string? PublishableKey => configuration["Stripe:PublishableKey"];
    private Uri? PublicBaseUrl => Uri.TryCreate(configuration["Stripe:PublicBaseUrl"], UriKind.Absolute, out var uri)
        && (uri.Scheme == Uri.UriSchemeHttps || uri.IsLoopback && uri.Scheme == Uri.UriSchemeHttp)
        ? uri : null;
    public bool IsConfigured => !string.IsNullOrWhiteSpace(Secret) && !string.IsNullOrWhiteSpace(PublishableKey)
        && !string.IsNullOrWhiteSpace(WebhookSecret) && PublicBaseUrl is not null;

    public async Task<StripeSession> CreateAsync(BikeRental rental, string returnPath, string language, CancellationToken token)
    {
        var fields = new Dictionary<string, string>
        {
            ["mode"] = "payment",
            ["ui_mode"] = "elements",
            ["payment_method_types[0]"] = "card",
            ["adaptive_pricing[enabled]"] = "false",
            ["client_reference_id"] = rental.Id.ToString(CultureInfo.InvariantCulture),
            ["line_items[0][quantity]"] = "1",
            ["line_items[0][price_data][currency]"] = "eur",
            ["line_items[0][price_data][unit_amount]"] = MinorUnits(rental.Price).ToString(CultureInfo.InvariantCulture),
            ["line_items[0][price_data][product_data][name]"] = rental.BikeModel,
            ["return_url"] = new Uri(PublicBaseUrl!, returnPath).AbsoluteUri,
            ["locale"] = language,
            ["expires_at"] = DateTimeOffset.UtcNow.AddMinutes(31).ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture)
        };
        using var request = new HttpRequestMessage(HttpMethod.Post, "checkout/sessions");
        request.Headers.Add("Idempotency-Key", $"bajkuj-{rental.Id}-{Guid.NewGuid():N}");
        request.Content = new FormUrlEncodedContent(fields);
        return await SendAsync(request, token);
    }

    public async Task<StripeSession> GetAsync(string sessionId, CancellationToken token)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "checkout/sessions/" + Uri.EscapeDataString(sessionId));
        return await SendAsync(request, token);
    }

    private async Task<StripeSession> SendAsync(HttpRequestMessage request, CancellationToken token)
    {
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", Secret);
        request.Headers.Add("Stripe-Version", "2026-11-18.endive");
        using var response = await client.SendAsync(request, token);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<StripeSession>(token)
            ?? throw new HttpRequestException("Stripe returned an empty Checkout Session.");
    }

    public bool IsValidWebhook(string signature, string body)
    {
        if (string.IsNullOrWhiteSpace(WebhookSecret))
            return false;

        var parts = signature.Split(',', StringSplitOptions.TrimEntries);
        var timestamp = parts.FirstOrDefault(part => part.StartsWith("t=", StringComparison.Ordinal));
        if (timestamp is null || !long.TryParse(timestamp.AsSpan(2), out var epoch))
            return false;
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        if (epoch < now - 300 || epoch > now + 300)
            return false;

        var expected = HMACSHA256.HashData(Encoding.UTF8.GetBytes(WebhookSecret),
            Encoding.UTF8.GetBytes(timestamp[2..] + "." + body));
        foreach (var part in parts.Where(part => part.StartsWith("v1=", StringComparison.Ordinal) && part.Length == 67))
        {
            try
            {
                if (CryptographicOperations.FixedTimeEquals(expected, Convert.FromHexString(part[3..])))
                    return true;
            }
            catch (FormatException) { }
        }
        return false;
    }

    public static async Task ApplySessionAsync(ApplicationDbContext db, StripeSession session, CancellationToken token)
    {
        if (!long.TryParse(session.Reference, out var id) || string.IsNullOrWhiteSpace(session.Id))
            return;

        var expected = await db.BikeRentals.AsNoTracking().SingleOrDefaultAsync(rental =>
            rental.Id == id && rental.CheckoutSessionId == session.Id && rental.PaymentPending, token);
        if (expected is null || session.Currency != "eur" || session.Amount != MinorUnits(expected.Price))
            return;

        var rentals = db.BikeRentals.Where(rental => rental.Id == id && rental.PaymentPending && rental.CancelledOn == null);
        if (session.Status == "complete" && session.PaymentStatus == "paid")
        {
            await rentals.ExecuteUpdateAsync(setters => setters.SetProperty(rental => rental.PaymentPending, false), token);
        }
        else if (session.Status == "expired" && session.PaymentStatus == "unpaid")
        {
            await using var transaction = await db.Database.BeginTransactionAsync(token);
            var today = BikeCatalogue.Today;
            var changed = await rentals.ExecuteUpdateAsync(setters => setters
                .SetProperty(rental => rental.PaymentPending, false)
                .SetProperty(rental => rental.CancelledOn, today), token);
            if (changed > 0)
                await db.BikeRentalDays.Where(day => day.BikeRentalId == id).ExecuteDeleteAsync(token);
            await transaction.CommitAsync(token);
        }
    }

    private static long MinorUnits(decimal price) => decimal.ToInt64(price * 100);
}

public sealed record StripeSession(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("client_secret")] string? ClientSecret,
    [property: JsonPropertyName("client_reference_id")] string? Reference,
    [property: JsonPropertyName("amount_total")] long? Amount,
    [property: JsonPropertyName("currency")] string? Currency,
    [property: JsonPropertyName("status")] string? Status,
    [property: JsonPropertyName("payment_status")] string? PaymentStatus);
