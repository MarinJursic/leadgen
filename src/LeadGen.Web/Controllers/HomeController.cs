using System.Diagnostics;
using LeadGen.Infrastructure.Data;
using LeadGen.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LeadGen.Web.Controllers;

public sealed class HomeController : Controller
{
    private readonly LeadGenDbContext _db;

    public HomeController(LeadGenDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var model = new DashboardViewModel(
            await _db.Campaigns.AsNoTracking().OrderByDescending(item => item.UpdatedAtUtc).Take(5).ToListAsync(ct),
            await _db.LeadSearchRuns.AsNoTracking().OrderByDescending(item => item.StartedAtUtc).Take(5).ToListAsync(ct),
            await _db.Leads.AsNoTracking().OrderByDescending(item => item.CreatedAtUtc).Take(5).ToListAsync(ct));
        return View(model);
    }

    public IActionResult About()
    {
        return View();
    }

    public IActionResult Error(string? correlationId)
    {
        return View(new ErrorViewModel
        {
            RequestId = correlationId ?? Activity.Current?.Id ?? HttpContext.TraceIdentifier
        });
    }
}
