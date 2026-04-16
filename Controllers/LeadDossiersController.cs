using leadgen.Services;
using leadgen.ViewModels.LeadDossiers;
using Microsoft.AspNetCore.Mvc;

namespace leadgen.Controllers;

public sealed class LeadDossiersController : Controller
{
    private readonly ILeadgenReadRepository _repository;

    public LeadDossiersController(ILeadgenReadRepository repository)
    {
        _repository = repository;
    }

    public IActionResult Index()
    {
        var dossiers = _repository.GetLeadDossiers()
            .OrderByDescending(dossier => dossier.LeadgenScore)
            .ThenByDescending(dossier => dossier.LastUpdatedAtUtc)
            .ToList();

        return View(dossiers);
    }

    public IActionResult Details(Guid id)
    {
        var dossier = _repository.GetLeadDossier(id);
        if (dossier is null)
        {
            return NotFound();
        }

        var run = _repository.GetMissionRun(dossier.MissionRunId);
        var mission = _repository.GetMissions().FirstOrDefault(item => item.Runs.Any(candidate => candidate.Id == dossier.MissionRunId));
        var company = _repository.GetTargetCompany(dossier.TargetCompanyId);
        var contact = _repository.GetTargetContact(dossier.TargetContactId);

        return View(new LeadDossierDetailsViewModel
        {
            Dossier = dossier,
            Run = run,
            Mission = mission,
            Company = company,
            Contact = contact
        });
    }
}
