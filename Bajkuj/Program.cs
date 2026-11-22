using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Bajkuj.Data;
using Bajkuj.Localization;
using Bajkuj.Models;
using Bajkuj.Payments;
using Microsoft.AspNetCore.Localization;
using MySqlConnector;

DotNetEnv.Env.TraversePath().Load();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var configuredConnection = builder.Configuration.GetConnectionString("DefaultConnection") ??
                           throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
var connectionString = new MySqlConnectionStringBuilder(configuredConnection) { AllowUserVariables = true }.ConnectionString;
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
        .ReplaceService<IMigrationsSqlGenerator, BajkujMigrationsSqlGenerator>());
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

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages()
    .WithStaticAssets();

app.Run();
