using leadgen.Services;
using leadgen.ViewModels.MissionRuns;
using Microsoft.AspNetCore.Mvc;

namespace leadgen.Controllers;

public sealed class MissionRunsController : Controller
{
    private readonly ILeadgenReadRepository _repository;

    public MissionRunsController(ILeadgenReadRepository repository)
    {
        _repository = repository;
    }

    public IActionResult Index()
    {
        var runs = _repository.GetMissionRuns()
            .OrderByDescending(run => run.StartedAtUtc)
            .ToList();

        return View(runs);
    }

    public IActionResult Details(Guid id)
    {
        var run = _repository.GetMissionRun(id);
        if (run is null)
        {
            return NotFound();
        }

        var mission = _repository.GetMissions().FirstOrDefault(item => item.Runs.Any(candidate => candidate.Id == id));

        return View(new MissionRunDetailsViewModel
        {
            Run = run,
            Mission = mission,
            Assignments = run.AgentAssignments,
            Companies = run.TargetCompanies,
            Dossiers = run.LeadDossiers
        });
    }
}
