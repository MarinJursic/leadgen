using LeadGen.Core.Domain;
using LeadGen.Infrastructure.Data;
using LeadGen.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LeadGen.Web.Controllers;

public sealed class LeadsController : Controller
{
    private readonly LeadGenDbContext _db;

    public LeadsController(LeadGenDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index(Guid? campaignId, CancellationToken ct)
    {
        ViewBag.CampaignId = campaignId;
        var query = _db.Leads.AsNoTracking()
            .Include(lead => lead.Campaign)
            .Include(lead => lead.Contacts)
            .AsQueryable();

        if (campaignId.HasValue)
        {
            query = query.Where(lead => lead.CampaignId == campaignId.Value);
        }

        var leads = await query.OrderByDescending(lead => lead.FitScore).ToListAsync(ct);
        return View(leads);
    }

    public async Task<IActionResult> Details(Guid id, CancellationToken ct)
    {
        var lead = await LoadLeadAsync(id, tracking: false, ct);
        if (lead is null)
        {
            return NotFound();
        }

        return View(new LeadDetailsViewModel(
            lead,
            new LeadContactFormModel { LeadId = lead.Id },
            new LeadNoteFormModel { LeadId = lead.Id }));
    }

    public async Task<IActionResult> Create(Guid? campaignId, CancellationToken ct)
    {
        await LoadCampaignsAsync(ct);
        return View(new LeadFormModel
        {
            CampaignId = campaignId ?? await _db.Campaigns.Select(campaign => campaign.Id).FirstOrDefaultAsync(ct),
            MatchReasonsJson = """["Manual lead"]""",
            EvidenceJson = """[{"title":"Manual source","url":"https://example.com","quoteOrSummary":"Added manually."}]""",
            DossierMarkdown = "Manual lead dossier."
        });
    }

    [HttpPost]
    public async Task<IActionResult> Create(LeadFormModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            await LoadCampaignsAsync(ct);
            return View(model);
        }

        var lead = new Lead();
        Apply(lead, model);
        lead.CreatedAtUtc = DateTime.UtcNow;
        lead.UpdatedAtUtc = DateTime.UtcNow;
        _db.Leads.Add(lead);
        await _db.SaveChangesAsync(ct);
        TempData["StatusMessage"] = "Lead saved.";
        return RedirectToAction(nameof(Details), new { id = lead.Id });
    }

    public async Task<IActionResult> Edit(Guid id, CancellationToken ct)
    {
        var lead = await _db.Leads.AsNoTracking().FirstOrDefaultAsync(item => item.Id == id, ct);
        if (lead is null)
        {
            return NotFound();
        }

        await LoadCampaignsAsync(ct);
        return View(ToForm(lead));
    }

    [HttpPost]
    public async Task<IActionResult> Edit(Guid id, LeadFormModel model, CancellationToken ct)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            await LoadCampaignsAsync(ct);
            return View(model);
        }

        var lead = await _db.Leads.FirstOrDefaultAsync(item => item.Id == id, ct);
        if (lead is null)
        {
            return NotFound();
        }

        Apply(lead, model);
        lead.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        TempData["StatusMessage"] = "Lead updated.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    public async Task<IActionResult> UpdateStatus(Guid id, LeadStatus status, CancellationToken ct)
    {
        var lead = await _db.Leads.FirstOrDefaultAsync(item => item.Id == id, ct);
        if (lead is null)
        {
            return NotFound();
        }

        lead.Status = status;
        lead.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        TempData["StatusMessage"] = "Lead status updated.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    public async Task<IActionResult> AddContact(LeadContactFormModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return RedirectToAction(nameof(Details), new { id = model.LeadId });
        }

        _db.LeadContacts.Add(new LeadContact
        {
            LeadId = model.LeadId,
            Type = model.Type,
            Value = model.Value.Trim(),
            SourceUrl = string.IsNullOrWhiteSpace(model.SourceUrl) ? null : model.SourceUrl.Trim(),
            ConfidenceScore = Math.Clamp(model.ConfidenceScore, 0, 100),
            IsVerified = model.IsVerified
        });
        await _db.SaveChangesAsync(ct);
        TempData["StatusMessage"] = "Contact added.";
        return RedirectToAction(nameof(Details), new { id = model.LeadId });
    }

    [HttpPost]
    public async Task<IActionResult> AddNote(LeadNoteFormModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return RedirectToAction(nameof(Details), new { id = model.LeadId });
        }

        _db.LeadNotes.Add(new LeadNote
        {
            LeadId = model.LeadId,
            Body = model.Body.Trim(),
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        });
        await _db.SaveChangesAsync(ct);
        TempData["StatusMessage"] = "Note added.";
        return RedirectToAction(nameof(Details), new { id = model.LeadId });
    }

    [HttpPost]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var lead = await _db.Leads.FirstOrDefaultAsync(item => item.Id == id, ct);
        if (lead is null)
        {
            return NotFound();
        }

        var campaignId = lead.CampaignId;
        _db.Leads.Remove(lead);
        await _db.SaveChangesAsync(ct);
        TempData["StatusMessage"] = "Lead deleted.";
        return RedirectToAction(nameof(Index), new { campaignId });
    }

    private async Task<Lead?> LoadLeadAsync(Guid id, bool tracking, CancellationToken ct)
    {
        var query = _db.Leads
            .Include(lead => lead.Campaign)
            .Include(lead => lead.Contacts)
            .Include(lead => lead.Notes)
            .AsQueryable();

        if (!tracking)
        {
            query = query.AsNoTracking();
        }

        return await query.FirstOrDefaultAsync(lead => lead.Id == id, ct);
    }

    private async Task LoadCampaignsAsync(CancellationToken ct)
    {
        ViewBag.Campaigns = await _db.Campaigns.AsNoTracking().OrderBy(campaign => campaign.Name).ToListAsync(ct);
    }

    private static LeadFormModel ToForm(Lead lead)
    {
        return new LeadFormModel
        {
            Id = lead.Id,
            CampaignId = lead.CampaignId,
            CompanyName = lead.CompanyName,
            Domain = lead.Domain,
            WebsiteUrl = lead.WebsiteUrl,
            Industry = lead.Industry,
            Location = lead.Location,
            FitScore = lead.FitScore,
            ConfidenceScore = lead.ConfidenceScore,
            Status = lead.Status,
            MatchReasonsJson = lead.MatchReasonsJson,
            EvidenceJson = lead.EvidenceJson,
            DossierMarkdown = lead.DossierMarkdown,
            SuggestedOutreachAngle = lead.SuggestedOutreachAngle
        };
    }

    private static void Apply(Lead lead, LeadFormModel model)
    {
        lead.CampaignId = model.CampaignId;
        lead.CompanyName = model.CompanyName.Trim();
        lead.Domain = NullIfWhiteSpace(model.Domain);
        lead.WebsiteUrl = NullIfWhiteSpace(model.WebsiteUrl);
        lead.Industry = NullIfWhiteSpace(model.Industry);
        lead.Location = NullIfWhiteSpace(model.Location);
        lead.FitScore = Math.Clamp(model.FitScore, 0, 100);
        lead.ConfidenceScore = Math.Clamp(model.ConfidenceScore, 0, 100);
        lead.Status = model.Status;
        lead.MatchReasonsJson = string.IsNullOrWhiteSpace(model.MatchReasonsJson) ? "[]" : model.MatchReasonsJson;
        lead.EvidenceJson = string.IsNullOrWhiteSpace(model.EvidenceJson) ? "[]" : model.EvidenceJson;
        lead.DossierMarkdown = model.DossierMarkdown.Trim();
        lead.SuggestedOutreachAngle = NullIfWhiteSpace(model.SuggestedOutreachAngle);
        lead.DedupeKey = LeadIdentity.BuildDedupeKey(lead.Domain, lead.CompanyName, lead.Location);
    }

    private static string? NullIfWhiteSpace(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
