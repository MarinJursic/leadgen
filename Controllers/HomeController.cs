using System.Diagnostics;
using leadgen.Models;
using leadgen.Services;
using Microsoft.AspNetCore.Mvc;

namespace leadgen.Controllers;

public sealed class HomeController : Controller
{
    private readonly ILeadgenDashboardService _dashboardService;

    public HomeController(ILeadgenDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    public IActionResult Index()
    {
        var model = _dashboardService.BuildDashboard();
        return View(model);
    }

    [HttpGet]
    public IActionResult Mission(string? dna)
    {
        var model = _dashboardService.BuildMissionCanvas(dna);
        return View(model);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
