using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Bajkuj.Data;
using Bajkuj.Localization;
using Bajkuj.Models;
using Bajkuj.Payments;
using Microsoft.AspNetCore.Localization;

DotNetEnv.Env.TraversePath().Load();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
                       throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddClaimsPrincipalFactory<AccountClaimsFactory>();
builder.Services.AddControllersWithViews(options => options.Filters.Add<LocalizedAccountErrors>());
builder.Services.AddHttpClient<StripeCheckout>(client =>
{
    client.BaseAddress = new Uri("https://api.stripe.com/v1/");
    client.Timeout = TimeSpan.FromSeconds(15);
});
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.SetDefaultCulture(AppText.Slovak)
        .AddSupportedCultures(AppText.Slovak, AppText.English)
        .AddSupportedUICultures(AppText.Slovak, AppText.English);
    options.RequestCultureProviders.Clear();
    options.RequestCultureProviders.Add(new CookieRequestCultureProvider());
});

var app = builder.Build();
if (args is ["--grant-admin", var email])
{
    await AdminAccounts.GrantAsync(app.Services, email);
    return;
}

app.UseRequestLocalization();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages()
    .WithStaticAssets();

app.Run();
