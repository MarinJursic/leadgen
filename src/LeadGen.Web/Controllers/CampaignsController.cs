using LeadGen.Core.Domain;
using LeadGen.Core.Services;
using LeadGen.Infrastructure.Data;
using LeadGen.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LeadGen.Web.Controllers;

public sealed class CampaignsController : Controller
{
    private readonly LeadGenDbContext _db;
    private readonly ILeadRunQueue _runQueue;

    public CampaignsController(
        LeadGenDbContext db,
        ILeadRunQueue runQueue)
    {
        _db = db;
        _runQueue = runQueue;
    }

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var campaigns = await _db.Campaigns
            .AsNoTracking()
            .Include(campaign => campaign.Runs)
            .Include(campaign => campaign.Leads)
            .OrderByDescending(campaign => campaign.UpdatedAtUtc)
            .ToListAsync(ct);
        return View(campaigns);
    }

    public IActionResult Create()
    {
        return View(new CampaignFormModel());
    }

    [HttpPost]
    public async Task<IActionResult> Create(CampaignFormModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var campaign = new Campaign();
        Apply(campaign, model);
        campaign.CreatedAtUtc = DateTime.UtcNow;
        campaign.UpdatedAtUtc = DateTime.UtcNow;
        _db.Campaigns.Add(campaign);
        await _db.SaveChangesAsync(ct);
        TempData["StatusMessage"] = "Campaign saved.";
        return RedirectToAction(nameof(Details), new { id = campaign.Id });
    }

    public async Task<IActionResult> Details(Guid id, CancellationToken ct)
    {
        var campaign = await _db.Campaigns
            .AsNoTracking()
            .Include(item => item.Runs.OrderByDescending(run => run.StartedAtUtc))
            .Include(item => item.Leads.OrderByDescending(lead => lead.FitScore))
            .ThenInclude(lead => lead.Contacts)
            .FirstOrDefaultAsync(item => item.Id == id, ct);

        return campaign is null ? NotFound() : View(campaign);
    }

    public async Task<IActionResult> Edit(Guid id, CancellationToken ct)
    {
        var campaign = await _db.Campaigns.AsNoTracking().FirstOrDefaultAsync(item => item.Id == id, ct);
        return campaign is null ? NotFound() : View(ToForm(campaign));
    }

    [HttpPost]
    public async Task<IActionResult> Edit(Guid id, CampaignFormModel model, CancellationToken ct)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var campaign = await _db.Campaigns.FirstOrDefaultAsync(item => item.Id == id, ct);
        if (campaign is null)
        {
            return NotFound();
        }

        Apply(campaign, model);
        campaign.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        TempData["StatusMessage"] = "Campaign updated.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    public async Task<IActionResult> StartRun(Guid id, int requestedLeadCount = 5, CancellationToken ct = default)
    {
        try
        {
            var runId = await _runQueue.EnqueueAsync(id, requestedLeadCount, ct);
            TempData["StatusMessage"] = "Lead discovery started. This page updates as the run progresses.";
            return RedirectToAction("Details", "Runs", new { id = runId });
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var campaign = await _db.Campaigns.AsNoTracking().FirstOrDefaultAsync(item => item.Id == id, ct);
        return campaign is null ? NotFound() : View(campaign);
    }

    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(Guid id, CancellationToken ct)
    {
        var campaign = await _db.Campaigns.FirstOrDefaultAsync(item => item.Id == id, ct);
        if (campaign is null)
        {
            return NotFound();
        }

        _db.Campaigns.Remove(campaign);
        await _db.SaveChangesAsync(ct);
        TempData["StatusMessage"] = "Campaign and related records deleted.";
        return RedirectToAction(nameof(Index));
    }

    private static CampaignFormModel ToForm(Campaign campaign)
    {
        return new CampaignFormModel
        {
            Id = campaign.Id,
            Name = campaign.Name,
            BusinessName = campaign.BusinessName,
            WebsiteUrl = campaign.WebsiteUrl,
            BusinessDescription = campaign.BusinessDescription,
            TargetGeography = campaign.TargetGeography,
            TargetCustomers = campaign.TargetCustomers,
            Exclusions = campaign.Exclusions,
            IcpJson = campaign.IcpJson
        };
    }

    private static void Apply(Campaign campaign, CampaignFormModel model)
    {
        campaign.Name = BuildCampaignName(model.Name, model.BusinessName);
        campaign.BusinessName = model.BusinessName.Trim();
        campaign.WebsiteUrl = NullIfWhiteSpace(model.WebsiteUrl);
        campaign.BusinessDescription = model.BusinessDescription.Trim();
        campaign.TargetGeography = NullIfWhiteSpace(model.TargetGeography);
        campaign.TargetCustomers = null;
        campaign.Exclusions = null;
        campaign.IcpJson = NullIfWhiteSpace(model.IcpJson);
    }

    private static string BuildCampaignName(string? name, string businessName)
    {
        if (!string.IsNullOrWhiteSpace(name))
        {
            return name.Trim();
        }

        return string.IsNullOrWhiteSpace(businessName)
            ? "Lead search campaign"
            : $"{businessName.Trim()} lead search";
    }

    private static string? NullIfWhiteSpace(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
