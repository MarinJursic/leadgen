using leadgen.Services;
using leadgen.ViewModels.TargetContacts;
using Microsoft.AspNetCore.Mvc;

namespace leadgen.Controllers;

public sealed class TargetContactsController : Controller
{
    private readonly ILeadgenReadRepository _repository;

    public TargetContactsController(ILeadgenReadRepository repository)
    {
        _repository = repository;
    }

    public IActionResult Index()
    {
        var contacts = _repository.GetTargetContacts()
            .OrderByDescending(contact => contact.IsDecisionMaker)
            .ThenBy(contact => contact.FullName)
            .ToList();

        return View(contacts);
    }

    public IActionResult Details(Guid id)
    {
        var contact = _repository.GetTargetContact(id);
        if (contact is null)
        {
            return NotFound();
        }

        var company = _repository.GetTargetCompanies().FirstOrDefault(item => item.Contacts.Any(candidate => candidate.Id == id));
        var run = _repository.GetMissionRuns().FirstOrDefault(item => item.TargetCompanies.Any(candidate => candidate.Contacts.Any(contactCandidate => contactCandidate.Id == id)));
        var mission = _repository.GetMissions().FirstOrDefault(item => item.Runs.Any(candidate => candidate.TargetCompanies.Any(companyCandidate => companyCandidate.Contacts.Any(contactCandidate => contactCandidate.Id == id))));
        var dossiers = run?.LeadDossiers.Where(dossier => dossier.TargetContactId == id).ToList() ?? [];

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
