using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Bajkuj.Models;

public class ApplicationUser : IdentityUser
{
    [MaxLength(80)] public string? FirstName { get; set; }
    [MaxLength(80)] public string? LastName { get; set; }
}

public class AccountClaimsFactory(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager,
    IOptions<IdentityOptions> options)
    : UserClaimsPrincipalFactory<ApplicationUser, IdentityRole>(userManager, roleManager, options)
{
    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
    {
        var identity = await base.GenerateClaimsAsync(user);
        if (!string.IsNullOrWhiteSpace(user.FirstName))
            identity.AddClaim(new Claim(ClaimTypes.GivenName, user.FirstName));
        if (!string.IsNullOrWhiteSpace(user.LastName))
            identity.AddClaim(new Claim(ClaimTypes.Surname, user.LastName));
        return identity;
    }
}

public static class AccountProfile
{
    public static string DisplayName(ClaimsPrincipal user)
    {
        var name = $"{user.FindFirstValue(ClaimTypes.GivenName)} {user.FindFirstValue(ClaimTypes.Surname)}".Trim();
        return name.Length > 0 ? name : Email(user);
    }

    public static string Email(ClaimsPrincipal user) => user.FindFirstValue(ClaimTypes.Email) ?? user.Identity?.Name ?? "";
}
