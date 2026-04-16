using leadgen.Services;
using leadgen.ViewModels.EvidencePoints;
using Microsoft.AspNetCore.Mvc;

namespace leadgen.Controllers;

public sealed class EvidencePointsController : Controller
{
    private readonly ILeadgenReadRepository _repository;

    public EvidencePointsController(ILeadgenReadRepository repository)
    {
        _repository = repository;
    }

    public IActionResult Index()
    {
        var evidencePoints = _repository.GetEvidencePoints()
            .OrderByDescending(evidence => evidence.CapturedAtUtc)
            .ToList();

        return View(evidencePoints);
    }

    public IActionResult Details(Guid id)
    {
        var evidence = _repository.GetEvidencePoint(id);
        if (evidence is null)
        {
            return NotFound();
        }

        var contact = _repository.GetTargetContacts().FirstOrDefault(item => item.EvidencePoints.Any(candidate => candidate.Id == id));
        var company = _repository.GetTargetCompanies().FirstOrDefault(item => item.Contacts.Any(contactItem => contactItem.EvidencePoints.Any(candidate => candidate.Id == id)));
        var mission = _repository.GetMissions().FirstOrDefault(item => item.Runs.Any(run => run.TargetCompanies.Any(companyItem => companyItem.Contacts.Any(contactItem => contactItem.EvidencePoints.Any(candidate => candidate.Id == id)))));

        return View(new EvidencePointDetailsViewModel
        {
            Evidence = evidence,
            Contact = contact,
            Company = company,
            Mission = mission
        });
    }
}
