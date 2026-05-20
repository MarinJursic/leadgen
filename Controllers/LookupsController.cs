using leadgen.Data;
using leadgen.ViewModels.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace leadgen.Controllers;

[Route("lookups")]
public sealed class LookupsController : Controller
{
    private readonly LeadgenDbContext _dbContext;

    public LookupsController(LeadgenDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet("missions")]
    public async Task<IActionResult> Missions(string? q)
    {
        var query = Normalize(q);
        var missions = await _dbContext.BusinessDnaMissions.AsNoTracking()
            .OrderBy(mission => mission.MissionName)
            .Take(200)
            .ToListAsync();

        return Json(missions
            .Where(mission => Matches(query, mission.MissionName, mission.ProductName, mission.Persona))
            .Take(12)
            .Select(mission => new AutocompleteOptionViewModel
            {
                Id = mission.Id,
                Text = mission.MissionName,
                Description = mission.ProductName
            }));
    }

    [HttpGet("runs")]
    public async Task<IActionResult> Runs(string? q)
    {
        var query = Normalize(q);
        var runs = await _dbContext.MissionRuns.AsNoTracking()
            .Include(run => run.Mission)
            .OrderByDescending(run => run.StartedAtUtc)
            .Take(200)
            .ToListAsync();

        return Json(runs
            .Where(run => Matches(query, run.RunCode, run.SearchRegion, run.Mission?.MissionName))
            .Take(12)
            .Select(run => new AutocompleteOptionViewModel
            {
                Id = run.Id,
                Text = run.RunCode,
                Description = run.Mission?.MissionName
            }));
    }

    [HttpGet("agents")]
    public async Task<IActionResult> Agents(string? q)
    {
        var query = Normalize(q);
        var agents = await _dbContext.SwarmAgents.AsNoTracking()
            .OrderBy(agent => agent.CodeName)
            .Take(200)
            .ToListAsync();

        return Json(agents
            .Where(agent => Matches(query, agent.CodeName, agent.Role.ToString(), agent.CurrentFocus))
            .Take(12)
            .Select(agent => new AutocompleteOptionViewModel
            {
                Id = agent.Id,
                Text = agent.CodeName,
                Description = agent.Role.ToString()
            }));
    }

    [HttpGet("companies")]
    public async Task<IActionResult> Companies(string? q)
    {
        var query = Normalize(q);
        var companies = await _dbContext.TargetCompanies.AsNoTracking()
            .OrderBy(company => company.Name)
            .Take(200)
            .ToListAsync();

        return Json(companies
            .Where(company => Matches(query, company.Name, company.Domain, company.Industry, company.HeadquartersCity))
            .Take(12)
            .Select(company => new AutocompleteOptionViewModel
            {
                Id = company.Id,
                Text = company.Name,
                Description = company.Domain
            }));
    }

    [HttpGet("contacts")]
    public async Task<IActionResult> Contacts(string? q)
    {
        var query = Normalize(q);
        var contacts = await _dbContext.TargetContacts.AsNoTracking()
            .Include(contact => contact.TargetCompany)
            .OrderBy(contact => contact.FullName)
            .Take(200)
            .ToListAsync();

        return Json(contacts
            .Where(contact => Matches(query, contact.FullName, contact.JobTitle, contact.Department, contact.TargetCompany?.Name))
            .Take(12)
            .Select(contact => new AutocompleteOptionViewModel
            {
                Id = contact.Id,
                Text = contact.FullName,
                Description = contact.TargetCompany?.Name
            }));
    }

    private static string Normalize(string? value) => (value ?? string.Empty).Trim().ToLowerInvariant();

    private static bool Matches(string query, params string?[] values)
    {
        return string.IsNullOrWhiteSpace(query)
            || values.Any(value => !string.IsNullOrWhiteSpace(value) && value.ToLowerInvariant().Contains(query));
    }
}
