using Bajkuj.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bajkuj.Controllers;

public class BikePhotoController(ApplicationDbContext db) : Controller
{
    [HttpGet("/bicykle/{id:int}/fotografia")]
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    public async Task<IActionResult> Show(int id, CancellationToken cancellationToken)
    {
        var photo = await db.BikePhotos.AsNoTracking()
            .SingleOrDefaultAsync(item => item.BicykelId == id, cancellationToken);
        if (photo is null)
            return NotFound();

        Response.Headers["X-Content-Type-Options"] = "nosniff";
        return File(photo.Content, photo.ContentType);
    }
}
