using leadgen.Data;
using leadgen.ViewModels.OutreachQueue;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace leadgen.Controllers;

[Route("outreach")]
public sealed class OutreachQueueController : Controller
{
    private readonly LeadgenDbContext _dbContext;

    public OutreachQueueController(LeadgenDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet("queue")]
    public async Task<IActionResult> Index()
    {
        var items = await _dbContext.LeadDossiers
            .AsNoTracking()
            .Where(dossier => dossier.IsReadyForOutreach)
            .OrderByDescending(dossier => dossier.LeadgenScore)
            .ThenByDescending(dossier => dossier.LastUpdatedAtUtc)
            .Select(dossier => new OutreachQueueItemViewModel
            {
                DossierId = dossier.Id,
                MissionName = dossier.MissionRun!.Mission!.MissionName,
                CompanyName = dossier.TargetCompany!.Name,
                ContactName = dossier.TargetContact!.FullName,
                LeadgenScore = dossier.LeadgenScore,
                AdvantagePoint = dossier.AdvantagePoint,
                SuggestedApproach = dossier.SuggestedApproach,
                LastUpdatedAtUtc = dossier.LastUpdatedAtUtc
            })
            .ToListAsync();

        return View(items);
    }
}
