using LeadGen.Core.Services;
using LeadGen.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LeadGen.Infrastructure.Services;

public sealed class GlobalSearchService : IGlobalSearchService
{
    private static readonly IReadOnlyList<GlobalSearchResult> MenuItems =
    [
        new("Menu", "Dashboard", "Recent campaigns, runs, and leads", "/"),
        new("Menu", "Campaigns", "Create and manage lead discovery campaigns", "/Campaigns"),
        new("Menu", "Leads", "Browse and review generated lead dossiers", "/Leads"),
        new("Menu", "Global search", "Search campaigns, leads, contacts, notes, and dossiers", "/Search"),
        new("Menu", "Admin logs", "Inspect safe application log tail", "/Admin/Logs"),
        new("Menu", "About", "Responsible-use boundaries and provider notes", "/Home/About")
    ];

    private readonly LeadGenDbContext _db;

    public GlobalSearchService(LeadGenDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<GlobalSearchResult>> SearchAsync(string? query, int take, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return [];
        }

        var term = query.Trim();
        var boundedTake = Math.Clamp(take, 1, 100);
        var results = new List<GlobalSearchResult>();

        results.AddRange(MenuItems
            .Where(item => Matches(item.Title, term) || Matches(item.Subtitle, term))
            .Take(10));

        var campaigns = await _db.Campaigns
            .AsNoTracking()
            .Where(campaign =>
                campaign.Name.Contains(term) ||
                campaign.BusinessName.Contains(term) ||
                campaign.BusinessDescription.Contains(term) ||
                (campaign.IcpJson != null && campaign.IcpJson.Contains(term)))
            .OrderByDescending(campaign => campaign.UpdatedAtUtc)
            .Take(20)
            .Select(campaign => new GlobalSearchResult(
                "Campaign",
                campaign.Name,
                campaign.BusinessName,
                $"/Campaigns/Details/{campaign.Id}"))
            .ToListAsync(ct);
        results.AddRange(campaigns);

        var leads = await _db.Leads
            .AsNoTracking()
            .Where(lead =>
                lead.CompanyName.Contains(term) ||
                (lead.Domain != null && lead.Domain.Contains(term)) ||
                lead.DossierMarkdown.Contains(term) ||
                lead.MatchReasonsJson.Contains(term))
            .OrderByDescending(lead => lead.FitScore)
            .Take(25)
            .Select(lead => new GlobalSearchResult(
                "Lead",
                lead.CompanyName,
                (lead.Domain ?? "No domain") + $" · Score {lead.FitScore}",
                $"/Leads/Details/{lead.Id}"))
            .ToListAsync(ct);
        results.AddRange(leads);

        var contacts = await _db.LeadContacts
            .AsNoTracking()
            .Include(contact => contact.Lead)
            .Where(contact => contact.Value.Contains(term))
            .OrderByDescending(contact => contact.CreatedAtUtc)
            .Take(20)
            .Select(contact => new GlobalSearchResult(
                "Contact",
                contact.Value,
                contact.Lead == null ? "Lead contact" : contact.Lead.CompanyName,
                contact.Lead == null ? "/Leads" : $"/Leads/Details/{contact.LeadId}"))
            .ToListAsync(ct);
        results.AddRange(contacts);

        var notes = await _db.LeadNotes
            .AsNoTracking()
            .Include(note => note.Lead)
            .Where(note => note.Body.Contains(term))
            .OrderByDescending(note => note.UpdatedAtUtc)
            .Take(20)
            .Select(note => new GlobalSearchResult(
                "Note",
                note.Body.Length > 70 ? note.Body.Substring(0, 70) + "..." : note.Body,
                note.Lead == null ? "Lead note" : note.Lead.CompanyName,
                note.Lead == null ? "/Leads" : $"/Leads/Details/{note.LeadId}"))
            .ToListAsync(ct);
        results.AddRange(notes);

        return results.Take(boundedTake).ToList();
    }

    private static bool Matches(string source, string term)
    {
        return source.Contains(term, StringComparison.OrdinalIgnoreCase);
    }
}
