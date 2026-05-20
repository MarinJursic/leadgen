using Leadgen.Model.Entities;
using leadgen.Data;
using leadgen.ViewModels.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace leadgen.Controllers;

[Route("search")]
public sealed class EntitySearchController : Controller
{
    private readonly LeadgenDbContext _dbContext;

    public EntitySearchController(LeadgenDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet("{entity}")]
    public async Task<IActionResult> Search(string entity, string? q)
    {
        var query = Normalize(q);
        var result = entity.ToLowerInvariant() switch
        {
            "missions" => await Missions(query),
            "questions" => await Questions(query),
            "runs" => await Runs(query),
            "assignments" => await Assignments(query),
            "agents" => await Agents(query),
            "companies" => await Companies(query),
            "contacts" => await Contacts(query),
            "channels" => await Channels(query),
            "evidence" => await Evidence(query),
            "dossiers" => await Dossiers(query),
            "queue" => await Queue(query),
            _ => new TableSearchResultViewModel()
        };

        return Json(result);
    }

    private async Task<TableSearchResultViewModel> Missions(string query)
    {
        var records = await _dbContext.BusinessDnaMissions.AsNoTracking()
            .Include(mission => mission.Runs)
            .OrderByDescending(mission => mission.ConfidenceScore)
            .ToListAsync();

        var rows = records
            .Where(mission => Matches(query, mission.MissionName, mission.ProductName, mission.Persona, mission.Status.ToString()))
            .Select(mission => Row(
                Cells(
                    Cell(mission.MissionName, mission.Persona),
                    Cell(mission.ProductName),
                    Cell(mission.Status.ToString()),
                    Cell(mission.ConfidenceScore.ToString("0.00")),
                    Cell(mission.Runs.Count.ToString())),
                EntityActions("Missions", mission.Id)));

        return Table(rows);
    }

    private async Task<TableSearchResultViewModel> Questions(string query)
    {
        var records = await _dbContext.ClarificationQuestions.AsNoTracking()
            .OrderByDescending(question => question.CreatedAtUtc)
            .ToListAsync();

        var rows = records
            .Where(question => Matches(query, question.SlotName, question.Prompt, question.Reason, question.Answer))
            .Select(question => Row(
                Cells(
                    Cell(question.SlotName),
                    Cell(question.Prompt),
                    Cell(question.IsAnswered ? "Yes" : "No"),
                    Cell(FormatDate(question.CreatedAtUtc))),
                EntityActions("ClarificationQuestions", question.Id)));

        return Table(rows);
    }

    private async Task<TableSearchResultViewModel> Runs(string query)
    {
        var records = await _dbContext.MissionRuns.AsNoTracking()
            .OrderByDescending(run => run.StartedAtUtc)
            .ToListAsync();

        var rows = records
            .Where(run => Matches(query, run.RunCode, run.Status.ToString(), run.SearchRegion))
            .Select(run => Row(
                Cells(
                    Cell(run.RunCode, FormatDate(run.StartedAtUtc)),
                    Cell(run.Status.ToString()),
                    Cell(run.SearchRegion),
                    Cell(run.TokenBudget.ToString()),
                    Cell($"${run.EstimatedCostUsd:0.00}")),
                EntityActions("MissionRuns", run.Id)));

        return Table(rows);
    }

    private async Task<TableSearchResultViewModel> Assignments(string query)
    {
        var records = await _dbContext.MissionAgentAssignments.AsNoTracking()
            .OrderByDescending(assignment => assignment.AssignedAtUtc)
            .ToListAsync();

        var rows = records
            .Where(assignment => Matches(query, assignment.Responsibility, assignment.Status.ToString()))
            .Select(assignment => Row(
                Cells(
                    Cell(assignment.Responsibility),
                    Cell(assignment.Status.ToString()),
                    Cell(FormatDateTime(assignment.AssignedAtUtc)),
                    Cell(assignment.TokenBudget.ToString())),
                EntityActions("MissionAgentAssignments", assignment.Id)));

        return Table(rows);
    }

    private async Task<TableSearchResultViewModel> Agents(string query)
    {
        var records = await _dbContext.SwarmAgents.AsNoTracking()
            .OrderBy(agent => agent.Role)
            .ThenBy(agent => agent.CodeName)
            .ToListAsync();

        var rows = records
            .Where(agent => Matches(query, agent.CodeName, agent.Role.ToString(), agent.Provider, agent.CurrentFocus))
            .Select(agent => Row(
                Cells(
                    Cell(agent.CodeName, agent.CurrentFocus),
                    Cell(agent.Role.ToString()),
                    Cell(agent.Provider),
                    Cell(agent.MaxConcurrentTasks.ToString())),
                EntityActions("SwarmAgents", agent.Id)));

        return Table(rows);
    }

    private async Task<TableSearchResultViewModel> Companies(string query)
    {
        var records = await _dbContext.TargetCompanies.AsNoTracking()
            .OrderByDescending(company => company.MatchScore)
            .ToListAsync();

        var rows = records
            .Where(company => Matches(query, company.Name, company.Domain, company.Industry, company.HeadquartersCity, company.HeadquartersCountry))
            .Select(company => Row(
                Cells(
                    Cell(company.Name, company.Domain),
                    Cell(company.Industry),
                    Cell($"{company.HeadquartersCity}, {company.HeadquartersCountry}"),
                    Cell(company.MatchScore.ToString("0.00"))),
                EntityActions("TargetCompanies", company.Id)));

        return Table(rows);
    }

    private async Task<TableSearchResultViewModel> Contacts(string query)
    {
        var records = await _dbContext.TargetContacts.AsNoTracking()
            .OrderByDescending(contact => contact.IsDecisionMaker)
            .ThenBy(contact => contact.FullName)
            .ToListAsync();

        var rows = records
            .Where(contact => Matches(query, contact.FullName, contact.JobTitle, contact.Department, contact.Seniority, contact.OpportunitySummary))
            .Select(contact => Row(
                Cells(
                    Cell(contact.FullName, contact.OpportunitySummary),
                    Cell(contact.JobTitle),
                    Cell(contact.Department),
                    Cell(contact.IsDecisionMaker ? "Yes" : "No")),
                EntityActions("TargetContacts", contact.Id)));

        return Table(rows);
    }

    private async Task<TableSearchResultViewModel> Channels(string query)
    {
        var records = await _dbContext.ContactChannels.AsNoTracking()
            .OrderByDescending(channel => channel.IsVerified)
            .ThenBy(channel => channel.Type)
            .ToListAsync();

        var rows = records
            .Where(channel => Matches(query, channel.Type.ToString(), channel.Value, channel.Source))
            .Select(channel => Row(
                Cells(
                    Cell(channel.Type.ToString()),
                    Cell(channel.Value),
                    Cell(channel.IsVerified ? "Yes" : "No"),
                    Cell(channel.Source)),
                EntityActions("ContactChannels", channel.Id)));

        return Table(rows);
    }

    private async Task<TableSearchResultViewModel> Evidence(string query)
    {
        var records = await _dbContext.EvidencePoints.AsNoTracking()
            .OrderByDescending(evidence => evidence.CapturedAtUtc)
            .ToListAsync();

        var rows = records
            .Where(evidence => Matches(query, evidence.Label, evidence.Kind.ToString(), evidence.SourcePlatform, evidence.Summary, evidence.RawSnippet))
            .Select(evidence => Row(
                Cells(
                    Cell(evidence.Label, evidence.Summary),
                    Cell(evidence.Kind.ToString()),
                    Cell(evidence.SourcePlatform),
                    Cell(FormatDate(evidence.CapturedAtUtc))),
                EntityActions("EvidencePoints", evidence.Id)));

        return Table(rows);
    }

    private async Task<TableSearchResultViewModel> Dossiers(string query)
    {
        var records = await _dbContext.LeadDossiers.AsNoTracking()
            .OrderByDescending(dossier => dossier.LeadgenScore)
            .ThenByDescending(dossier => dossier.LastUpdatedAtUtc)
            .ToListAsync();

        var rows = records
            .Where(dossier => Matches(query, dossier.AdvantagePoint, dossier.SuggestedApproach, dossier.IsReadyForOutreach ? "ready" : "not ready"))
            .Select(dossier => Row(
                Cells(
                    Cell(dossier.LeadgenScore.ToString()),
                    Cell(dossier.AdvantagePoint),
                    Cell(dossier.IsReadyForOutreach ? "Yes" : "No"),
                    Cell(FormatDate(dossier.LastUpdatedAtUtc))),
                EntityActions("LeadDossiers", dossier.Id)));

        return Table(rows);
    }

    private async Task<TableSearchResultViewModel> Queue(string query)
    {
        var records = await _dbContext.LeadDossiers.AsNoTracking()
            .Include(dossier => dossier.MissionRun)!.ThenInclude(run => run!.Mission)
            .Include(dossier => dossier.TargetCompany)
            .Include(dossier => dossier.TargetContact)
            .Where(dossier => dossier.IsReadyForOutreach)
            .OrderByDescending(dossier => dossier.LeadgenScore)
            .ThenByDescending(dossier => dossier.LastUpdatedAtUtc)
            .ToListAsync();

        var cards = records
            .Where(dossier => Matches(query, dossier.MissionRun?.Mission?.MissionName, dossier.TargetCompany?.Name, dossier.TargetContact?.FullName, dossier.AdvantagePoint, dossier.SuggestedApproach))
            .Select(dossier => new OutreachSearchCardViewModel
            {
                Title = dossier.TargetContact?.FullName ?? "Unknown contact",
                Subtitle = dossier.TargetCompany?.Name ?? "Unknown company",
                Meta = $"Updated {FormatDateTime(dossier.LastUpdatedAtUtc)} UTC",
                Score = $"Score {dossier.LeadgenScore}",
                Summary = dossier.AdvantagePoint,
                DetailUrl = Url.Action("Details", "LeadDossiers", new { id = dossier.Id }) ?? "#"
            })
            .Take(50)
            .ToList();

        return new TableSearchResultViewModel
        {
            TotalCount = cards.Count,
            Cards = cards
        };
    }

    private IReadOnlyList<TableSearchActionViewModel> EntityActions(string controller, Guid id)
    {
        return
        [
            Action("Details", Url.Action("Details", controller, new { id }) ?? "#"),
            Action("Edit", Url.Action("Edit", controller, new { id }) ?? "#"),
            Action("Delete", Url.Action("Delete", controller, new { id }) ?? "#", "danger")
        ];
    }

    private static TableSearchResultViewModel Table(IEnumerable<TableSearchRowViewModel> rows)
    {
        var rowList = rows.Take(50).ToList();
        return new TableSearchResultViewModel
        {
            TotalCount = rowList.Count,
            Rows = rowList
        };
    }

    private static TableSearchRowViewModel Row(IReadOnlyList<TableSearchCellViewModel> cells, IReadOnlyList<TableSearchActionViewModel> actions)
    {
        return new TableSearchRowViewModel
        {
            Cells = cells,
            Actions = actions
        };
    }

    private static IReadOnlyList<TableSearchCellViewModel> Cells(params TableSearchCellViewModel[] cells) => cells;

    private static TableSearchCellViewModel Cell(string primary, string? secondary = null) => new()
    {
        Primary = primary,
        Secondary = secondary
    };

    private static TableSearchActionViewModel Action(string label, string url, string style = "") => new()
    {
        Label = label,
        Url = url,
        Style = style
    };

    private static string FormatDate(DateTime value) => value.ToString("yyyy-MM-dd");

    private static string FormatDateTime(DateTime value) => value.ToString("yyyy-MM-dd HH:mm");

    private static string Normalize(string? value) => (value ?? string.Empty).Trim().ToLowerInvariant();

    private static bool Matches(string query, params string?[] values)
    {
        return string.IsNullOrWhiteSpace(query)
            || values.Any(value => !string.IsNullOrWhiteSpace(value) && value.ToLowerInvariant().Contains(query));
    }
}
