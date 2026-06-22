using LeadGen.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LeadGen.Web.Controllers;

public sealed class RunsController : Controller
{
    private readonly LeadGenDbContext _db;

    public RunsController(LeadGenDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Details(Guid id, CancellationToken ct)
    {
        var run = await _db.LeadSearchRuns
            .AsNoTracking()
            .Include(item => item.Campaign)
            .Include(item => item.Leads.OrderByDescending(lead => lead.FitScore))
            .FirstOrDefaultAsync(item => item.Id == id, ct);
        return run is null ? NotFound() : View(run);
    }
}
