using Bajkuj.Models;
using Microsoft.AspNetCore.Identity;

namespace Bajkuj.Data;

public static class AdminAccounts
{
    public const string Role = "Admin";

    public static async Task GrantAsync(IServiceProvider services, string email)
    {
        await using var scope = services.CreateAsyncScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roles = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        var user = await users.FindByNameAsync(email.Trim());
        if (user is null)
        {
            Console.Error.WriteLine("Register that account in Bajkuj before granting the Admin role.");
            Environment.ExitCode = 1;
            return;
        }

        if (!await roles.RoleExistsAsync(Role))
        {
            var created = await roles.CreateAsync(new IdentityRole(Role));
            if (!created.Succeeded && !await roles.RoleExistsAsync(Role))
            {
                Console.Error.WriteLine("The Admin role could not be created.");
                Environment.ExitCode = 1;
                return;
            }
        }

        if (!await users.IsInRoleAsync(user, Role) && !(await users.AddToRoleAsync(user, Role)).Succeeded)
        {
            Console.Error.WriteLine("The Admin role could not be assigned.");
            Environment.ExitCode = 1;
            return;
        }

        Console.WriteLine("Admin access granted. Sign out and sign in again to see the Admin view.");
    }
}
