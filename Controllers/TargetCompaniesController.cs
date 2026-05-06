using leadgen.Services;
using leadgen.ViewModels.TargetCompanies;
using Microsoft.AspNetCore.Mvc;

namespace leadgen.Controllers;

// Expose target company list and details pages.
[Route("companies")]
public sealed class TargetCompaniesController : Controller
{
    // Read-only access to the seeded Leadgen dataset.
    private readonly ILeadgenReadRepository _repository;

    // Receive the repository from dependency injection.
    public TargetCompaniesController(ILeadgenReadRepository repository)
    {
        _repository = repository;
    }

    // Show all target companies ordered by best match score first.
    [HttpGet("")]
    public IActionResult Index()
    {
        var companies = _repository.GetTargetCompanies()
            .OrderByDescending(company => company.MatchScore)
            .ToList();

        return View(companies);
    }

    // Show one target company plus mission, run, and related dossiers.
    [HttpGet("{id:guid}")]
    public IActionResult Details(Guid id)
    {
        var company = _repository.GetTargetCompany(id);
        if (company is null)
        {
            return NotFound();
        }

        // Resolve the owning run, mission, and company-linked dossiers.
        var run = _repository.GetMissionRuns().FirstOrDefault(item => item.TargetCompanies.Any(candidate => candidate.Id == id));
        var mission = _repository.GetMissions().FirstOrDefault(item => item.Runs.Any(candidate => candidate.TargetCompanies.Any(companyCandidate => companyCandidate.Id == id)));
        var dossiers = run?.LeadDossiers.Where(dossier => dossier.TargetCompanyId == id).ToList() ?? [];

        // Send the assembled view model to the details page.
        return View(new TargetCompanyDetailsViewModel
        {
            Company = company,
            Mission = mission,
            Run = run,
            Dossiers = dossiers
        });
    }
}
