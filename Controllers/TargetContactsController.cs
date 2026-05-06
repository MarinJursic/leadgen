using leadgen.Services;
using leadgen.ViewModels.TargetContacts;
using Microsoft.AspNetCore.Mvc;

namespace leadgen.Controllers;

// Expose target contact list and details pages.
[Route("contacts")]
public sealed class TargetContactsController : Controller
{
    // Read-only access to the seeded Leadgen dataset.
    private readonly ILeadgenReadRepository _repository;

    // Receive the repository from dependency injection.
    public TargetContactsController(ILeadgenReadRepository repository)
    {
        _repository = repository;
    }

    // Show all contacts with decision-makers first.
    [HttpGet("")]
    public IActionResult Index()
    {
        var contacts = _repository.GetTargetContacts()
            .OrderByDescending(contact => contact.IsDecisionMaker)
            .ThenBy(contact => contact.FullName)
            .ToList();

        return View(contacts);
    }

    // Show one contact plus company, run, mission, and related dossiers.
    [HttpGet("{id:guid}")]
    public IActionResult Details(Guid id)
    {
        var contact = _repository.GetTargetContact(id);
        if (contact is null)
        {
            return NotFound();
        }

        // Resolve the surrounding object-graph context for this contact.
        var company = _repository.GetTargetCompanies().FirstOrDefault(item => item.Contacts.Any(candidate => candidate.Id == id));
        var run = _repository.GetMissionRuns().FirstOrDefault(item => item.TargetCompanies.Any(candidate => candidate.Contacts.Any(contactCandidate => contactCandidate.Id == id)));
        var mission = _repository.GetMissions().FirstOrDefault(item => item.Runs.Any(candidate => candidate.TargetCompanies.Any(companyCandidate => companyCandidate.Contacts.Any(contactCandidate => contactCandidate.Id == id))));
        var dossiers = run?.LeadDossiers.Where(dossier => dossier.TargetContactId == id).ToList() ?? [];

        // Send the assembled details model to the view.
        return View(new TargetContactDetailsViewModel
        {
            Contact = contact,
            Company = company,
            Mission = mission,
            Run = run,
            Dossiers = dossiers
        });
    }
}
