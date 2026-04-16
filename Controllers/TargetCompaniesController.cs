using leadgen.Services;
using leadgen.ViewModels.TargetCompanies;
using Microsoft.AspNetCore.Mvc;

namespace leadgen.Controllers;

public sealed class TargetCompaniesController : Controller
{
    private readonly ILeadgenReadRepository _repository;

    public TargetCompaniesController(ILeadgenReadRepository repository)
    {
        _repository = repository;
    }

    public IActionResult Index()
    {
        var companies = _repository.GetTargetCompanies()
            .OrderByDescending(company => company.MatchScore)
            .ToList();

        return View(companies);
    }

    public IActionResult Details(Guid id)
    {
        var company = _repository.GetTargetCompany(id);
        if (company is null)
        {
            return NotFound();
        }

        var run = _repository.GetMissionRuns().FirstOrDefault(item => item.TargetCompanies.Any(candidate => candidate.Id == id));
        var mission = _repository.GetMissions().FirstOrDefault(item => item.Runs.Any(candidate => candidate.TargetCompanies.Any(companyCandidate => companyCandidate.Id == id)));
        var dossiers = run?.LeadDossiers.Where(dossier => dossier.TargetCompanyId == id).ToList() ?? [];

        return View(new TargetCompanyDetailsViewModel
        {
            Company = company,
            Mission = mission,
            Run = run,
            Dossiers = dossiers
        });
    }
}
