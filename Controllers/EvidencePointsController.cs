using leadgen.Services;
using leadgen.ViewModels.EvidencePoints;
using Microsoft.AspNetCore.Mvc;

namespace leadgen.Controllers;

// Expose evidence list and details pages.
public sealed class EvidencePointsController : Controller
{
    // Read-only access to the seeded Leadgen dataset.
    private readonly ILeadgenReadRepository _repository;

    // Receive the repository from dependency injection.
    public EvidencePointsController(ILeadgenReadRepository repository)
    {
        _repository = repository;
    }

    // Show all evidence points ordered by most recently captured first.
    public IActionResult Index()
    {
        var evidencePoints = _repository.GetEvidencePoints()
            .OrderByDescending(evidence => evidence.CapturedAtUtc)
            .ToList();

        return View(evidencePoints);
    }

    // Show one evidence point plus the contact, company, and mission above it.
    public IActionResult Details(Guid id)
    {
        var evidence = _repository.GetEvidencePoint(id);
        if (evidence is null)
        {
            return NotFound();
        }

        // Resolve the evidence's place in the nested object graph.
        var contact = _repository.GetTargetContacts().FirstOrDefault(item => item.EvidencePoints.Any(candidate => candidate.Id == id));
        var company = _repository.GetTargetCompanies().FirstOrDefault(item => item.Contacts.Any(contactItem => contactItem.EvidencePoints.Any(candidate => candidate.Id == id)));
        var mission = _repository.GetMissions().FirstOrDefault(item => item.Runs.Any(run => run.TargetCompanies.Any(companyItem => companyItem.Contacts.Any(contactItem => contactItem.EvidencePoints.Any(candidate => candidate.Id == id)))));

        // Send the assembled details model to the view.
        return View(new EvidencePointDetailsViewModel
        {
            Evidence = evidence,
            Contact = contact,
            Company = company,
            Mission = mission
        });
    }
}
