using Bajkuj.Models;
using Microsoft.AspNetCore.Mvc;

namespace Bajkuj.Controllers;

public class PrototypeController : Controller
{
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

        if (role == "admin" && section == "index")
        {
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        ViewData["PrototypeRole"] = role;
        ViewData["PrototypeSection"] = section;
        var folder = role == "admin" ? "Admin" : "User";
        return View($"~/Views/{folder}/{section}.cshtml");
    }
}
