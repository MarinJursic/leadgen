using System.Diagnostics;
using leadgen.Models;
using leadgen.Services;
using Microsoft.AspNetCore.Mvc;

namespace leadgen.Controllers;

// Serve the custom landing page, mission canvas, and standard shared pages.
public sealed class HomeController : Controller
{
    // Aggregate cross-entity data for the home and mission views.
    private readonly ILeadgenDashboardService _dashboardService;

    // Receive the dashboard service from dependency injection.
    public HomeController(ILeadgenDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    // Render the custom landing page with dashboard data.
    [HttpGet("/")]
    public IActionResult Index()
    {
        // Build the typed view model used by the home view.
        var model = _dashboardService.BuildDashboard();
        return View(model);
    }

    [HttpGet("/mission-control")]
    // Render the mission canvas using optional DNA text from the query string.
    public IActionResult Mission(string? dna)
    {
        // Build the typed mission-canvas model from the submitted DNA.
        var model = _dashboardService.BuildMissionCanvas(dna);
        return View(model);
    }

    // Return the placeholder privacy page from the MVC template.
    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    // Return the standard error page with a traceable request id.
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
