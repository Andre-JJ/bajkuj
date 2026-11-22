using System.ComponentModel.DataAnnotations;
using Bajkuj.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Bajkuj.Controllers;

[Authorize]
public class ProfileController(UserManager<ApplicationUser> users, SignInManager<ApplicationUser> signIn) : Controller
{
    [HttpPost("/profil/upravit", Name = "edit-profile")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit([FromForm] ProfileInput input, [FromForm] string? role)
    {
        var user = await users.GetUserAsync(User);
        if (user is null)
            return Challenge();

        if (!ModelState.IsValid)
            return ReturnToSettings(role, "Zadajte platné meno, priezvisko a e-mail.", error: true);

        var email = input.Email.Trim();
        var emailChanged = !string.Equals(user.Email, email, StringComparison.OrdinalIgnoreCase);
        if (emailChanged && (string.IsNullOrEmpty(input.Password) || !await users.CheckPasswordAsync(user, input.Password)))
            return ReturnToSettings(role, "Na zmenu e-mailu zadajte správne aktuálne heslo.", error: true);

        user.FirstName = input.FirstName.Trim();
        user.LastName = input.LastName.Trim();
        if (emailChanged)
        {
            user.Email = email;
            user.UserName = email;
            user.EmailConfirmed = false;
        }

        var result = emailChanged ? await users.UpdateSecurityStampAsync(user) : await users.UpdateAsync(user);
        if (!result.Succeeded)
            return ReturnToSettings(role, "Profil sa nepodarilo uložiť. Skontrolujte e-mail a skúste znova.", error: true);

        await signIn.RefreshSignInAsync(user);
        return ReturnToSettings(role, "Profil bol uložený.", error: false);
    }

    private IActionResult ReturnToSettings(string? role, string message, bool error)
    {
        TempData[error ? "ProfileError" : "ProfileSaved"] = message;
        return RedirectToRoute("prototype", new
        {
            role = role == "admin" && User.IsInRole(Bajkuj.Data.AdminAccounts.Role) ? "admin" : "user",
            section = "nastavenia"
        });
    }

    public class ProfileInput
    {
        [Required, StringLength(80)] public string FirstName { get; set; } = "";
        [Required, EmailAddress, StringLength(254)] public string Email { get; set; } = "";
        [Required, StringLength(80)] public string LastName { get; set; } = "";
        public string? Password { get; set; }
    }
}
