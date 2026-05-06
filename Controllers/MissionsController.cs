using leadgen.Services;
using leadgen.ViewModels.Missions;
using Microsoft.AspNetCore.Mvc;

namespace leadgen.Controllers;

// Expose mission list and mission details pages.
[Route("missions")]
public sealed class MissionsController : Controller
{
    // Read-only access to the seeded Leadgen dataset.
    private readonly ILeadgenReadRepository _repository;

    // Receive the repository from dependency injection.
    public MissionsController(ILeadgenReadRepository repository)
    {
        _repository = repository;
    }

    // Show all missions ordered by highest confidence first.
    [HttpGet("")]
    public IActionResult Index()
    {
        // Load and sort the missions for the index table.
        var missions = _repository.GetMissions()
            .OrderByDescending(mission => mission.ConfidenceScore)
            .ToList();

        return View(missions);
    }

    // Show one mission plus its latest run and aggregate output counts.
    [HttpGet("{id:guid}")]
    public IActionResult Details(Guid id)
    {
        // Find the requested mission by id.
        var mission = _repository.GetMission(id);
        if (mission is null)
        {
            return NotFound();
        }

        // Pick the newest run so the details page can summarize the latest execution.
        var latestRun = mission.Runs
            .OrderByDescending(run => run.StartedAtUtc)
            .FirstOrDefault();

        // Build a UI-specific view model with derived counts.
        var model = new MissionDetailsViewModel
        {
            Mission = mission,
            LatestRun = latestRun,
            CompanyCount = mission.Runs.SelectMany(run => run.TargetCompanies).Count(),
            ContactCount = mission.Runs.SelectMany(run => run.TargetCompanies).SelectMany(company => company.Contacts).Count(),
            DossierCount = mission.Runs.SelectMany(run => run.LeadDossiers).Count()
        };

        return View(model);
    }
}
