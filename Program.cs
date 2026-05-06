using leadgen.Data;
using leadgen.Services;
using Microsoft.EntityFrameworkCore;

// Create the ASP.NET Core application builder from command-line arguments.
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Enable MVC controllers plus Razor views.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<LeadgenDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("LeadgenDb")));
// Register the read-only repository over the EF-backed dataset.
builder.Services.AddScoped<ILeadgenReadRepository, LeadgenEfRepository>();
// Register the dashboard service used by the custom home and mission pages.
builder.Services.AddScoped<ILeadgenDashboardService, LeadgenDashboardService>();

// Build the configured web application.
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
// Enable endpoint routing for controller actions.
app.UseRouting();

app.UseAuthorization();

// Expose CSS, JS, and other static assets from the web project.
app.MapStaticAssets();
app.MapControllers().WithStaticAssets();

// Map the default MVC route so empty URLs open Home/Index.
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<LeadgenDbContext>();
    dbContext.Database.Migrate();
    await LeadgenDbSeeder.SeedAsync(dbContext);
}

// Start the HTTP server.
app.Run();
