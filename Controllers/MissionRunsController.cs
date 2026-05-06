using leadgen.Services;
using leadgen.ViewModels.MissionRuns;
using Microsoft.AspNetCore.Mvc;

namespace leadgen.Controllers;

// Expose mission run list and details pages.
public sealed class MissionRunsController : Controller
{
    // Read-only access to the seeded Leadgen dataset.
    private readonly ILeadgenReadRepository _repository;

    // Receive the repository from dependency injection.
    public MissionRunsController(ILeadgenReadRepository repository)
    {
        _repository = repository;
    }

    // Show all mission runs ordered by most recent start time.
    [HttpGet]
    public IActionResult Index()
    {
        var runs = _repository.GetMissionRuns()
            .OrderByDescending(run => run.StartedAtUtc)
            .ToList();

        return View(runs);
    }

    // Show one mission run and its related mission, assignments, companies, and dossiers.
    public IActionResult Details(Guid id)
    {
        var run = _repository.GetMissionRun(id);
        if (run is null)
        {
            return NotFound();
        }

        // Find the mission that contains this run.
        var mission = _repository.GetMissions().FirstOrDefault(item => item.Runs.Any(candidate => candidate.Id == id));

        // Package the run and its outputs into a UI-specific view model.
        return View(new MissionRunDetailsViewModel
        {
            Run = run,
            Mission = mission,
            Assignments = run.AgentAssignments.ToList(),
            Companies = run.TargetCompanies.ToList(),
            Dossiers = run.LeadDossiers.ToList()
        });
    }
}
