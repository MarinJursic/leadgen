using leadgen.Services;
using leadgen.ViewModels.LeadDossiers;
using Microsoft.AspNetCore.Mvc;

namespace leadgen.Controllers;

// Expose final lead dossier list and details pages.
[Route("dossiers")]
public sealed class LeadDossiersController : Controller
{
    // Read-only access to the seeded Leadgen dataset.
    private readonly ILeadgenReadRepository _repository;

    // Receive the repository from dependency injection.
    public LeadDossiersController(ILeadgenReadRepository repository)
    {
        _repository = repository;
    }

    // Show all dossiers ordered by highest score, then most recently updated.
    [HttpGet("")]
    public IActionResult Index()
    {
        var dossiers = _repository.GetLeadDossiers()
            .OrderByDescending(dossier => dossier.LeadgenScore)
            .ThenByDescending(dossier => dossier.LastUpdatedAtUtc)
            .ToList();

        return View(dossiers);
    }

    // Show one dossier plus its run, mission, company, and contact context.
    [HttpGet("{id:guid}")]
    public IActionResult Details(Guid id)
    {
        var dossier = _repository.GetLeadDossier(id);
        if (dossier is null)
        {
            return NotFound();
        }

        // Resolve the surrounding context using the dossier's linked ids.
        var run = _repository.GetMissionRun(dossier.MissionRunId);
        var mission = _repository.GetMissions().FirstOrDefault(item => item.Runs.Any(candidate => candidate.Id == dossier.MissionRunId));
        var company = _repository.GetTargetCompany(dossier.TargetCompanyId);
        var contact = _repository.GetTargetContact(dossier.TargetContactId);

        // Send the assembled details model to the view.
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
