using System.Data;
using System.Globalization;
using Bajkuj.Data;
using Bajkuj.Localization;
using Bajkuj.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bajkuj.Controllers;

[Authorize(Roles = AdminAccounts.Role)]
[Route("/sprava/bicykle")]
public class BikeInventoryController(ApplicationDbContext db) : Controller
{
    private const int PhotoLimit = 2 * 1024 * 1024;
    private const int FormLimit = PhotoLimit + 64 * 1024;
    private const int MaxUnits = 10000;
    private static readonly string[] StandardTypes = ["Mestský", "Horský", "Trekingový", "Elektrobicykel"];

    [HttpGet("novy", Name = "inventory-create")]
    public async Task<IActionResult> Create(CancellationToken cancellationToken) =>
        ShowForm(new BikeInventoryForm(), await GetTypesAsync(cancellationToken));

    [HttpPost("novy")]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(FormLimit)]
    public async Task<IActionResult> Create(BikeInventoryForm form, CancellationToken cancellationToken)
    {
        var types = await GetTypesAsync(cancellationToken);
        var values = await ReadFormAsync(form, types, isEdit: false, cancellationToken);
        if (values is null)
            return ShowForm(form, types);

        var bike = new Bicykel { DatumPridania = BikeCatalogue.Today.ToDateTime(TimeOnly.MinValue) };
        ApplyDetails(bike, form, values);
        bike.CustomPhoto = values.Photo;
        AddUnits(bike, form.Units!.Value, form.Status!);
        db.Bicykle.Add(bike);
        await db.SaveChangesAsync(cancellationToken);
        return ToCatalogue("Bicykel bol pridaný.");
    }

    [HttpGet("{id:int}/upravit", Name = "inventory-edit")]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var bike = await db.Bicykle.AsNoTracking().Include(model => model.Units)
            .SingleOrDefaultAsync(model => model.Id == id, cancellationToken);
        if (bike is null)
            return NotFound();

        var statuses = bike.Units.Select(unit => unit.Stav).Distinct().ToList();
        return ShowForm(new BikeInventoryForm
        {
            Name = bike.Model,
            Type = bike.Typ,
            Status = statuses.Count == 1 ? statuses[0] : null,
            Units = bike.Units.Count,
            MonthlyPrice = bike.CenaZaMesiac.ToString("0.00", CultureInfo.CurrentCulture),
            YearlyPrice = bike.CenaZaRok.ToString("0.00", CultureInfo.CurrentCulture)
        }, await GetTypesAsync(cancellationToken), bike);
    }

    [HttpPost("{id:int}/upravit")]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(FormLimit)]
    public async Task<IActionResult> Edit([FromRoute] int id, BikeInventoryForm form, CancellationToken cancellationToken)
    {
        var types = await GetTypesAsync(cancellationToken);
        var values = await ReadFormAsync(form, types, isEdit: true, cancellationToken);
        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
        var bike = await BikeCatalogue.LockModelAsync(db, id, cancellationToken);
        if (bike is null)
            return NotFound();
        if (values is null)
            return ShowForm(form, types, bike);

        await db.Entry(bike).Collection(item => item.Units).LoadAsync(cancellationToken);
        var difference = form.Units!.Value - bike.Units.Count;
        if (difference < 0)
        {
            var reservedIds = await ReservedUnits(bike.Id).ToHashSetAsync(cancellationToken);
            var removed = bike.Units.Where(unit => !reservedIds.Contains(unit.Id))
                .OrderByDescending(unit => unit.Id).Take(-difference).ToList();
            if (removed.Count != -difference)
            {
                AddError(nameof(form.Units), "Počet kusov nemôže byť nižší ako počet rezervovaných kusov.");
                return ShowForm(form, types, bike);
            }
            foreach (var unit in removed)
                bike.Units.Remove(unit);
            db.BikeUnits.RemoveRange(removed);
        }
        else if (difference > 0)
        {
            AddUnits(bike, difference, form.Status ?? BikeUnit.Available);
        }

        ApplyDetails(bike, form, values);
        if (form.Status is string status)
        {
            foreach (var unit in bike.Units)
                unit.Stav = status;
        }
        if (values.Photo is not null)
        {
            var photo = await db.BikePhotos.FindAsync([id], cancellationToken);
            if (photo is null)
                bike.CustomPhoto = values.Photo;
            else
            {
                photo.Content = values.Photo.Content;
                photo.ContentType = values.Photo.ContentType;
            }
        }
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return ToCatalogue("Zmeny bicykla boli uložené.");
    }

    [HttpPost("{id:int}/odstranit", Name = "inventory-delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete([FromRoute] int id, CancellationToken cancellationToken)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
        var bike = await BikeCatalogue.LockModelAsync(db, id, cancellationToken);
        if (bike is null)
            return NotFound();
        if (await ReservedUnits(bike.Id).AnyAsync(cancellationToken))
            return ToCatalogue("Bicykel s aktívnym prenájmom alebo platbou nemožno odstrániť.");

        db.Bicykle.Remove(bike);
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return ToCatalogue("Bicykel bol odstránený.");
    }

    private IQueryable<int> ReservedUnits(int modelId)
    {
        var today = BikeCatalogue.Today;
        return from rental in db.BikeRentals
               join unit in db.BikeUnits on rental.BikeUnitId equals (int?)unit.Id
               where unit.BicykelId == modelId && rental.CancelledOn == null &&
                   (rental.PaymentPending || rental.ReturnDate > today)
               select unit.Id;
    }

    private async Task<string[]> GetTypesAsync(CancellationToken cancellationToken)
    {
        var existing = await db.Bicykle.Select(bike => bike.Typ).Distinct().ToListAsync(cancellationToken);
        return StandardTypes.Union(existing, StringComparer.Ordinal).OrderBy(type => AppText.T(type)).ToArray();
    }

    private async Task<InventoryValues?> ReadFormAsync(BikeInventoryForm form, string[] types, bool isEdit,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(form.Name) || form.Name.Trim().Length > 128)
            AddError(nameof(form.Name), "Zadajte názov bicykla s najviac 128 znakmi.");
        if (form.Type is null || !types.Contains(form.Type, StringComparer.Ordinal))
            AddError(nameof(form.Type), "Vyberte typ bicykla zo zoznamu.");
        if (form.Units is null or < 1 or > MaxUnits)
            AddError(nameof(form.Units), "Zadajte počet kusov od 1 do 10 000.");
        if (!(isEdit && form.Status is null) && form.Status is not (BikeUnit.Available or BikeUnit.Rented or BikeUnit.InService))
            AddError(nameof(form.Status), "Vyberte platný stav bicykla.");
        if (!TryParsePrice(form.MonthlyPrice, out var monthly))
            AddError(nameof(form.MonthlyPrice), "Zadajte platnú mesačnú cenu v eurách.");
        if (!TryParsePrice(form.YearlyPrice, out var yearly))
            AddError(nameof(form.YearlyPrice), "Zadajte platnú ročnú cenu v eurách.");
        else if (monthly > 0 && yearly >= monthly * 12)
            AddError(nameof(form.YearlyPrice), "Ročná cena musí byť nižšia ako dvanásť mesačných platieb.");

        BikePhoto? photo = null;
        if (form.Photo is { } file)
        {
            if (file.Length is > 0 and <= PhotoLimit)
            {
                using var buffer = new MemoryStream((int)file.Length);
                await file.CopyToAsync(buffer, cancellationToken);
                var bytes = buffer.ToArray();
                var mime = PhotoType(bytes);
                if (mime is not null)
                    photo = new BikePhoto { Content = bytes, ContentType = mime };
            }
            if (photo is null)
                AddError(nameof(form.Photo), "Nahrajte obrázok JPG, PNG alebo WebP do 2 MB.");
        }

        return ModelState.IsValid ? new InventoryValues(monthly, yearly, photo) : null;
    }

    private static string? PhotoType(byte[] bytes)
    {
        var data = bytes.AsSpan();
        if (data.Length < 12)
            return null;
        if (data[..3].SequenceEqual(new byte[] { 0xff, 0xd8, 0xff }) && data[^2..].SequenceEqual(new byte[] { 0xff, 0xd9 }))
            return "image/jpeg";
        if (data[..8].SequenceEqual(new byte[] { 0x89, 0x50, 0x4e, 0x47, 0x0d, 0x0a, 0x1a, 0x0a }) &&
            data.Length >= 45 && data.Slice(12, 4).SequenceEqual("IHDR"u8) && data.Slice(data.Length - 8, 4).SequenceEqual("IEND"u8))
            return "image/png";
        if (data.Length >= 20 && data[..4].SequenceEqual("RIFF"u8) && data.Slice(8, 4).SequenceEqual("WEBP"u8) &&
            System.Buffers.Binary.BinaryPrimitives.ReadUInt32LittleEndian(data.Slice(4, 4)) == data.Length - 8)
            return "image/webp";
        return null;
    }

    private static bool TryParsePrice(string? text, out decimal price) =>
        (decimal.TryParse(text?.Trim(), NumberStyles.AllowDecimalPoint, CultureInfo.GetCultureInfo(AppText.Slovak), out price) ||
            decimal.TryParse(text?.Trim(), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out price))
        && price is >= .50m and <= 999999.99m && decimal.Round(price, 2) == price;

    private static void AddUnits(Bicykel bike, int count, string status)
    {
        for (var i = 0; i < count; i++)
            bike.Units.Add(new BikeUnit { Stav = status });
    }

    private static void ApplyDetails(Bicykel bike, BikeInventoryForm form, InventoryValues values)
    {
        bike.Model = form.Name!.Trim();
        bike.Typ = form.Type!.Trim();
        bike.CenaZaMesiac = values.MonthlyPrice;
        bike.CenaZaRok = values.YearlyPrice;
        bike.HasCustomPhoto |= values.Photo is not null;
    }

    private ViewResult ShowForm(BikeInventoryForm form, string[] types, Bicykel? bike = null)
    {
        ViewData["PrototypeRole"] = "admin";
        ViewData["PrototypeSection"] = "bicykle";
        ViewData["BikeId"] = bike?.Id;
        ViewData["PhotoPath"] = bike?.PhotoPath;
        ViewData["BikeTypes"] = types;
        return View("~/Views/Admin/bicykel-formular.cshtml", form);
    }

    private void AddError(string field, string message) => ModelState.AddModelError(field, AppText.T(message));

    private IActionResult ToCatalogue(string message)
    {
        TempData["InventoryNotice"] = message;
        return RedirectToRoute("prototype", new { role = "admin", section = "bicykle" });
    }

    private sealed record InventoryValues(decimal MonthlyPrice, decimal YearlyPrice, BikePhoto? Photo);
}
