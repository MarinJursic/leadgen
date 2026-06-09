using Leadgen.Model.Entities;
using leadgen.Data;
using leadgen.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

// Create the ASP.NET Core application builder from command-line arguments.
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Enable MVC controllers plus Razor views.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddDbContext<LeadgenDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("LeadgenDb")));
builder.Services
    .AddIdentity<AppUser, IdentityRole>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<LeadgenDbContext>()
    .AddDefaultTokenProviders();
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/account/login";
    options.AccessDeniedPath = "/account/access-denied";
});

var googleClientId = builder.Configuration["Authentication:Google:ClientId"];
var googleClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
if (!string.IsNullOrWhiteSpace(googleClientId) && !string.IsNullOrWhiteSpace(googleClientSecret))
{
    builder.Services.AddAuthentication()
        .AddGoogle(options =>
        {
            options.ClientId = googleClientId;
            options.ClientSecret = googleClientSecret;
        });
}

// Register the read-only repository over the EF-backed dataset.
builder.Services.AddScoped<ILeadgenReadRepository, LeadgenEfRepository>();
// Register the dashboard service used by the custom home and mission pages.
builder.Services.AddScoped<ILeadgenDashboardService, LeadgenDashboardService>();

// Build the configured web application.
var app = builder.Build();
var supportedCultures = new[]
{
    new CultureInfo("hr"),
    new CultureInfo("hr-HR"),
    new CultureInfo("en"),
    new CultureInfo("en-US")
};
var localizationOptions = new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("hr-HR"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
};

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRequestLocalization(localizationOptions);
// Enable endpoint routing for controller actions.
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Expose CSS, JS, and other static assets from the web project.
app.MapStaticAssets();
app.MapControllers().WithStaticAssets();

// Map the default MVC route so empty URLs open Home/Index.
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();
app.MapRazorPages().WithStaticAssets();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<LeadgenDbContext>();
    dbContext.Database.Migrate();
    await LeadgenDbSeeder.SeedAsync(dbContext);
    await LeadgenIdentitySeeder.SeedAsync(scope.ServiceProvider);
}

// Start the HTTP server.
app.Run();

public partial class Program
{
}
