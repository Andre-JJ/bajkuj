using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using Bajkuj.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace Bajkuj.Areas.Identity.Pages.Account;

[AllowAnonymous]
public class RegisterAccountModel(UserManager<ApplicationUser> users, SignInManager<ApplicationUser> signIn,
    IEmailSender<ApplicationUser> emailSender) : PageModel
{
    [BindProperty]
    public RegistrationInput Input { get; set; } = new();
    public string ReturnUrl { get; private set; } = "";
    public IEnumerable<AuthenticationScheme> ExternalLogins { get; private set; } = [];

    public class RegistrationInput
    {
        [Required, StringLength(80), Display(Name = "First name")]
        public string FirstName { get; set; } = "";

        [Required, StringLength(80), Display(Name = "Last name")]
        public string LastName { get; set; } = "";

        [Required, EmailAddress]
        public string Email { get; set; } = "";

        [Required, DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.")]
        public string Password { get; set; } = "";

        [Required, DataType(DataType.Password), Display(Name = "Confirm password")]
        [Compare(nameof(Password), ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = "";
    }

    public async Task OnGetAsync(string? returnUrl) => await LoadFormAsync(returnUrl);

    public async Task<IActionResult> OnPostAsync(string? returnUrl)
    {
        await LoadFormAsync(returnUrl);
        if (!ModelState.IsValid)
            return Page();

        var user = new ApplicationUser
        {
            UserName = Input.Email,
            Email = Input.Email,
            FirstName = Input.FirstName.Trim(),
            LastName = Input.LastName.Trim()
        };
        var result = await users.CreateAsync(user, Input.Password);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);
            return Page();
        }

        var code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(await users.GenerateEmailConfirmationTokenAsync(user)));
        var confirmationUrl = Url.Page("/Account/ConfirmEmail", null,
            new { area = "Identity", userId = user.Id, code, returnUrl = ReturnUrl }, Request.Scheme)!;
        await emailSender.SendConfirmationLinkAsync(user, Input.Email, HtmlEncoder.Default.Encode(confirmationUrl));

        if (!await signIn.CanSignInAsync(user))
            return RedirectToPage("./RegisterConfirmation", new { email = user.Email, returnUrl = ReturnUrl });

        await signIn.SignInAsync(user, isPersistent: false);
        return LocalRedirect(ReturnUrl);
    }

    private async Task LoadFormAsync(string? returnUrl)
    {
        ReturnUrl = Url.IsLocalUrl(returnUrl) ? returnUrl! : Url.Content("~/");
        ExternalLogins = await signIn.GetExternalAuthenticationSchemesAsync();
    }
}
