using LeadGen.Core.Configuration;
using LeadGen.Core.Domain;
using LeadGen.Core.Services;
using LeadGen.Infrastructure.Data;
using LeadGen.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace LeadGen.Web.Controllers;

[ApiController]
[Route("api")]
public sealed class LeadGenApiController : ControllerBase
{
    private readonly LeadGenDbContext _db;
    private readonly ILeadDiscoveryWorkflow _workflow;
    private readonly IGlobalSearchService _search;
    private readonly IAppLogReader _logs;
    private readonly LeadGenOptions _options;

    public LeadGenApiController(
        LeadGenDbContext db,
        ILeadDiscoveryWorkflow workflow,
        IGlobalSearchService search,
        IAppLogReader logs,
        IOptions<LeadGenOptions> options)
    {
        _db = db;
        _workflow = workflow;
        _search = search;
        _logs = logs;
        _options = options.Value;
    }

    [HttpGet("health")]
    public IActionResult Health()
    {
        var configured = !string.IsNullOrWhiteSpace(_options.DeepSeekApiKey)
            && !string.IsNullOrWhiteSpace(_options.TavilyApiKey);
        return Ok(new { status = "OK", provider = "Real", configured, correlationId = CorrelationId });
    }

    [HttpGet("campaigns")]
    public async Task<ActionResult<IReadOnlyList<CampaignDto>>> ListCampaigns(CancellationToken ct)
    {
        var campaigns = await _db.Campaigns
            .AsNoTracking()
            .Include(campaign => campaign.Runs)
            .Include(campaign => campaign.Leads)
            .OrderByDescending(campaign => campaign.UpdatedAtUtc)
            .Select(campaign => ToDto(campaign))
            .ToListAsync(ct);
        return Ok(campaigns);
    }

    [HttpPost("campaigns")]
    public async Task<ActionResult<CampaignDto>> CreateCampaign([FromBody] CampaignWriteRequest request, CancellationToken ct)
    {
        var campaign = new Campaign();
        Apply(campaign, request);
        campaign.CreatedAtUtc = DateTime.UtcNow;
        campaign.UpdatedAtUtc = DateTime.UtcNow;
        _db.Campaigns.Add(campaign);
        await _db.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(GetCampaign), new { id = campaign.Id }, ToDto(campaign));
    }

    [HttpGet("campaigns/{id:guid}")]
    public async Task<ActionResult<CampaignDto>> GetCampaign(Guid id, CancellationToken ct)
    {
        var campaign = await _db.Campaigns
            .AsNoTracking()
            .Include(item => item.Runs)
            .Include(item => item.Leads)
            .FirstOrDefaultAsync(item => item.Id == id, ct);
        return campaign is null ? NotFoundError("campaign_not_found", "Campaign was not found.") : Ok(ToDto(campaign));
    }

    [HttpPut("campaigns/{id:guid}")]
    public async Task<ActionResult<CampaignDto>> UpdateCampaign(Guid id, [FromBody] CampaignWriteRequest request, CancellationToken ct)
    {
        var campaign = await _db.Campaigns.FirstOrDefaultAsync(item => item.Id == id, ct);
        if (campaign is null)
        {
            return NotFoundError("campaign_not_found", "Campaign was not found.");
        }

        Apply(campaign, request);
        campaign.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return Ok(ToDto(campaign));
    }

    [HttpDelete("campaigns/{id:guid}")]
    public async Task<IActionResult> DeleteCampaign(Guid id, CancellationToken ct)
    {
        var campaign = await _db.Campaigns.FirstOrDefaultAsync(item => item.Id == id, ct);
        if (campaign is null)
        {
            return NotFoundError("campaign_not_found", "Campaign was not found.");
        }

        _db.Campaigns.Remove(campaign);
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpPost("campaigns/{id:guid}/generate-icp")]
    public async Task<IActionResult> GenerateIcp(Guid id, CancellationToken ct)
    {
        try
        {
            var icp = await _workflow.GenerateIcpAsync(id, ct);
            return Ok(new { campaignId = id, icp });
        }
        catch (KeyNotFoundException)
        {
            return NotFoundError("campaign_not_found", "Campaign was not found.");
        }
    }

    [HttpPost("campaigns/{id:guid}/runs")]
    public async Task<ActionResult<RunDto>> StartRun(Guid id, [FromBody] StartRunRequest? request, CancellationToken ct)
    {
        try
        {
            var summary = await _workflow.StartRunAsync(id, request?.RequestedLeadCount ?? 5, ct);
            var run = await LoadRunAsync(summary.RunId, ct);
            return CreatedAtAction(nameof(GetRun), new { id = summary.RunId }, ToDto(run!));
        }
        catch (KeyNotFoundException)
        {
            return NotFoundError("campaign_not_found", "Campaign was not found.");
        }
    }

    [HttpGet("runs/{id:guid}")]
    public async Task<ActionResult<RunDto>> GetRun(Guid id, CancellationToken ct)
    {
        var run = await LoadRunAsync(id, ct);
        return run is null ? NotFoundError("run_not_found", "Run was not found.") : Ok(ToDto(run));
    }

    [HttpGet("leads")]
    public async Task<ActionResult<IReadOnlyList<LeadDto>>> ListLeads([FromQuery] Guid? campaignId, CancellationToken ct)
    {
        var query = _db.Leads.AsNoTracking()
            .Include(lead => lead.Contacts)
            .Include(lead => lead.Notes)
            .AsQueryable();

        if (campaignId.HasValue)
        {
            query = query.Where(lead => lead.CampaignId == campaignId.Value);
        }

        var leads = await query
            .OrderByDescending(lead => lead.FitScore)
            .Select(lead => ToDto(lead))
            .ToListAsync(ct);
        return Ok(leads);
    }

    [HttpGet("leads/{id:guid}")]
    public async Task<ActionResult<LeadDto>> GetLead(Guid id, CancellationToken ct)
    {
        var lead = await LoadLeadAsync(id, tracking: false, ct);
        return lead is null ? NotFoundError("lead_not_found", "Lead was not found.") : Ok(ToDto(lead));
    }

    [HttpPost("leads")]
    public async Task<ActionResult<LeadDto>> CreateLead([FromBody] LeadWriteRequest request, CancellationToken ct)
    {
        if (!await _db.Campaigns.AnyAsync(item => item.Id == request.CampaignId, ct))
        {
            return BadRequestError("campaign_not_found", "Campaign was not found.");
        }

        var lead = new Lead
        {
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };
        Apply(lead, request);
        _db.Leads.Add(lead);
        await _db.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(GetLead), new { id = lead.Id }, ToDto(lead));
    }

    [HttpPut("leads/{id:guid}")]
    public async Task<ActionResult<LeadDto>> UpdateLead(Guid id, [FromBody] LeadWriteRequest request, CancellationToken ct)
    {
        var lead = await LoadLeadAsync(id, tracking: true, ct);
        if (lead is null)
        {
            return NotFoundError("lead_not_found", "Lead was not found.");
        }

        if (!await _db.Campaigns.AnyAsync(item => item.Id == request.CampaignId, ct))
        {
            return BadRequestError("campaign_not_found", "Campaign was not found.");
        }

        Apply(lead, request);
        lead.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return Ok(ToDto(lead));
    }

    [HttpDelete("leads/{id:guid}")]
    public async Task<IActionResult> DeleteLead(Guid id, CancellationToken ct)
    {
        var lead = await _db.Leads.FirstOrDefaultAsync(item => item.Id == id, ct);
        if (lead is null)
        {
            return NotFoundError("lead_not_found", "Lead was not found.");
        }

        _db.Leads.Remove(lead);
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpPost("leads/{id:guid}/contacts")]
    public async Task<ActionResult<LeadContactDto>> AddContact(Guid id, [FromBody] LeadContactWriteRequest request, CancellationToken ct)
    {
        if (!await _db.Leads.AnyAsync(item => item.Id == id, ct))
        {
            return NotFoundError("lead_not_found", "Lead was not found.");
        }

        var contact = new LeadContact { LeadId = id };
        Apply(contact, request);
        _db.LeadContacts.Add(contact);
        await _db.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(GetLead), new { id }, ToDto(contact));
    }

    [HttpPut("contacts/{id:guid}")]
    public async Task<ActionResult<LeadContactDto>> UpdateContact(Guid id, [FromBody] LeadContactWriteRequest request, CancellationToken ct)
    {
        var contact = await _db.LeadContacts.FirstOrDefaultAsync(item => item.Id == id, ct);
        if (contact is null)
        {
            return NotFoundError("contact_not_found", "Contact was not found.");
        }

        Apply(contact, request);
        await _db.SaveChangesAsync(ct);
        return Ok(ToDto(contact));
    }

    [HttpDelete("contacts/{id:guid}")]
    public async Task<IActionResult> DeleteContact(Guid id, CancellationToken ct)
    {
        var contact = await _db.LeadContacts.FirstOrDefaultAsync(item => item.Id == id, ct);
        if (contact is null)
        {
            return NotFoundError("contact_not_found", "Contact was not found.");
        }

        _db.LeadContacts.Remove(contact);
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpPost("leads/{id:guid}/notes")]
    public async Task<ActionResult<LeadNoteDto>> AddNote(Guid id, [FromBody] LeadNoteWriteRequest request, CancellationToken ct)
    {
        if (!await _db.Leads.AnyAsync(item => item.Id == id, ct))
        {
            return NotFoundError("lead_not_found", "Lead was not found.");
        }

        var note = new LeadNote
        {
            LeadId = id,
            Body = request.Body.Trim(),
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };
        _db.LeadNotes.Add(note);
        await _db.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(GetLead), new { id }, ToDto(note));
    }

    [HttpPut("notes/{id:guid}")]
    public async Task<ActionResult<LeadNoteDto>> UpdateNote(Guid id, [FromBody] LeadNoteWriteRequest request, CancellationToken ct)
    {
        var note = await _db.LeadNotes.FirstOrDefaultAsync(item => item.Id == id, ct);
        if (note is null)
        {
            return NotFoundError("note_not_found", "Note was not found.");
        }

        note.Body = request.Body.Trim();
        note.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return Ok(ToDto(note));
    }

    [HttpDelete("notes/{id:guid}")]
    public async Task<IActionResult> DeleteNote(Guid id, CancellationToken ct)
    {
        var note = await _db.LeadNotes.FirstOrDefaultAsync(item => item.Id == id, ct);
        if (note is null)
        {
            return NotFoundError("note_not_found", "Note was not found.");
        }

        _db.LeadNotes.Remove(note);
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpGet("search")]
    public async Task<ActionResult<IReadOnlyList<Models.SearchResultDto>>> Search([FromQuery] string? q, CancellationToken ct)
    {
        var results = await _search.SearchAsync(q, 50, ct);
        return Ok(results.Select(item => new Models.SearchResultDto(item.Type, item.Title, item.Subtitle, item.Url)).ToList());
    }

    [HttpGet("logs")]
    public async Task<IActionResult> Logs([FromQuery] int take = 100, CancellationToken ct = default)
    {
        if (!_options.EnableAdminLogViewer)
        {
            return NotFoundError("logs_disabled", "Log viewer is disabled.");
        }

        var lines = await _logs.TailAsync(take, ct);
        return Ok(new { lines });
    }

    private string CorrelationId => HttpContext.Items.TryGetValue("CorrelationId", out var id) && id is string value
        ? value
        : HttpContext.TraceIdentifier;

    private ObjectResult Error(int statusCode, string code, string message)
    {
        return StatusCode(statusCode, new ApiErrorEnvelope(new ApiError(code, message, CorrelationId)));
    }

    private ObjectResult NotFoundError(string code, string message) => Error(StatusCodes.Status404NotFound, code, message);

    private ObjectResult BadRequestError(string code, string message) => Error(StatusCodes.Status400BadRequest, code, message);

    private async Task<LeadSearchRun?> LoadRunAsync(Guid id, CancellationToken ct)
    {
        return await _db.LeadSearchRuns
            .AsNoTracking()
            .Include(run => run.Leads)
            .FirstOrDefaultAsync(run => run.Id == id, ct);
    }

    private async Task<Lead?> LoadLeadAsync(Guid id, bool tracking, CancellationToken ct)
    {
        var query = _db.Leads
            .Include(lead => lead.Contacts)
            .Include(lead => lead.Notes)
            .AsQueryable();

        if (!tracking)
        {
            query = query.AsNoTracking();
        }

        return await query.FirstOrDefaultAsync(lead => lead.Id == id, ct);
    }

    private static CampaignDto ToDto(Campaign campaign)
    {
        return new CampaignDto(
            campaign.Id,
            campaign.Name,
            campaign.BusinessName,
            campaign.WebsiteUrl,
            campaign.BusinessDescription,
            campaign.TargetGeography,
            campaign.TargetCustomers,
            campaign.Exclusions,
            campaign.IcpJson,
            campaign.CreatedAtUtc,
            campaign.UpdatedAtUtc,
            campaign.Runs.Count,
            campaign.Leads.Count);
    }

    private static RunDto ToDto(LeadSearchRun run)
    {
        return new RunDto(
            run.Id,
            run.CampaignId,
            run.Status,
            run.RequestedLeadCount,
            run.SearchQueriesJson,
            run.StartedAtUtc,
            run.CompletedAtUtc,
            run.ErrorMessage,
            run.EstimatedCostUsd,
            run.LogsJson,
            run.Leads.Count);
    }

    private static LeadDto ToDto(Lead lead)
    {
        return new LeadDto(
            lead.Id,
            lead.CampaignId,
            lead.LeadSearchRunId,
            lead.CompanyName,
            lead.Domain,
            lead.WebsiteUrl,
            lead.Industry,
            lead.Location,
            lead.FitScore,
            lead.ConfidenceScore,
            lead.Status,
            lead.MatchReasonsJson,
            lead.EvidenceJson,
            lead.DossierMarkdown,
            lead.SuggestedOutreachAngle,
            lead.Contacts.OrderByDescending(contact => contact.ConfidenceScore).Select(ToDto).ToList(),
            lead.Notes.OrderByDescending(note => note.CreatedAtUtc).Select(ToDto).ToList());
    }

    private static LeadContactDto ToDto(LeadContact contact)
    {
        return new LeadContactDto(
            contact.Id,
            contact.LeadId,
            contact.Type,
            contact.Value,
            contact.SourceUrl,
            contact.ConfidenceScore,
            contact.IsVerified);
    }

    private static LeadNoteDto ToDto(LeadNote note)
    {
        return new LeadNoteDto(note.Id, note.LeadId, note.Body, note.CreatedAtUtc, note.UpdatedAtUtc);
    }

    private static void Apply(Campaign campaign, CampaignWriteRequest request)
    {
        campaign.Name = BuildCampaignName(request.Name, request.BusinessName);
        campaign.BusinessName = request.BusinessName.Trim();
        campaign.WebsiteUrl = NullIfWhiteSpace(request.WebsiteUrl);
        campaign.BusinessDescription = request.BusinessDescription.Trim();
        campaign.TargetGeography = NullIfWhiteSpace(request.TargetGeography);
        campaign.TargetCustomers = NullIfWhiteSpace(request.TargetCustomers);
        campaign.Exclusions = NullIfWhiteSpace(request.Exclusions);
        campaign.IcpJson = NullIfWhiteSpace(request.IcpJson);
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

    private static void Apply(Lead lead, LeadWriteRequest request)
    {
        lead.CampaignId = request.CampaignId;
        lead.CompanyName = request.CompanyName.Trim();
        lead.Domain = NullIfWhiteSpace(request.Domain);
        lead.WebsiteUrl = NullIfWhiteSpace(request.WebsiteUrl);
        lead.Industry = NullIfWhiteSpace(request.Industry);
        lead.Location = NullIfWhiteSpace(request.Location);
        lead.FitScore = Math.Clamp(request.FitScore, 0, 100);
        lead.ConfidenceScore = Math.Clamp(request.ConfidenceScore, 0, 100);
        lead.Status = request.Status;
        lead.MatchReasonsJson = string.IsNullOrWhiteSpace(request.MatchReasonsJson) ? "[]" : request.MatchReasonsJson;
        lead.EvidenceJson = string.IsNullOrWhiteSpace(request.EvidenceJson) ? "[]" : request.EvidenceJson;
        lead.DossierMarkdown = request.DossierMarkdown.Trim();
        lead.SuggestedOutreachAngle = NullIfWhiteSpace(request.SuggestedOutreachAngle);
        lead.DedupeKey = LeadIdentity.BuildDedupeKey(lead.Domain, lead.CompanyName, lead.Location);
    }

    private static void Apply(LeadContact contact, LeadContactWriteRequest request)
    {
        contact.Type = request.Type;
        contact.Value = request.Value.Trim();
        contact.SourceUrl = NullIfWhiteSpace(request.SourceUrl);
        contact.ConfidenceScore = Math.Clamp(request.ConfidenceScore, 0, 100);
        contact.IsVerified = request.IsVerified;
    }

    private static string? NullIfWhiteSpace(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
