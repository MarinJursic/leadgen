using leadgen.Services;
using leadgen.ViewModels.Missions;
using Microsoft.AspNetCore.Mvc;

namespace leadgen.Controllers;

public sealed class MissionsController : Controller
{
    private readonly ILeadgenReadRepository _repository;

    public MissionsController(ILeadgenReadRepository repository)
    {
        _repository = repository;
    }

    public IActionResult Index()
    {
        var missions = _repository.GetMissions()
            .OrderByDescending(mission => mission.ConfidenceScore)
            .ToList();

        return View(missions);
    }

    public IActionResult Details(Guid id)
    {
        var mission = _repository.GetMission(id);
        if (mission is null)
        {
            return NotFound();
        }

        var latestRun = mission.Runs
            .OrderByDescending(run => run.StartedAtUtc)
            .FirstOrDefault();

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
