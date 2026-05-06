using leadgen.Services;
using leadgen.ViewModels.ContactChannels;
using Microsoft.AspNetCore.Mvc;

namespace leadgen.Controllers;

// Expose contact channel list and details pages.
public sealed class ContactChannelsController : Controller
{
    // Read-only access to the seeded Leadgen dataset.
    private readonly ILeadgenReadRepository _repository;

    // Receive the repository from dependency injection.
    public ContactChannelsController(ILeadgenReadRepository repository)
    {
        _repository = repository;
    }

    // Show all contact channels with verified channels first.
    public IActionResult Index()
    {
        var channels = _repository.GetContactChannels()
            .OrderByDescending(channel => channel.IsVerified)
            .ThenBy(channel => channel.Type)
            .ToList();

        return View(channels);
    }

    // Show one contact channel and the contact/company that owns it.
    public IActionResult Details(Guid id)
    {
        var channel = _repository.GetContactChannel(id);
        if (channel is null)
        {
            return NotFound();
        }

        // Resolve the owning contact and company through the object graph.
        var contact = _repository.GetTargetContacts().FirstOrDefault(item => item.ContactChannels.Any(candidate => candidate.Id == id));
        var company = _repository.GetTargetCompanies().FirstOrDefault(item => item.Contacts.Any(contactItem => contactItem.ContactChannels.Any(candidate => candidate.Id == id)));

        // Send the assembled details model to the view.
        return View(new ContactChannelDetailsViewModel
        {
            Channel = channel,
            Contact = contact,
            Company = company
        });
    }
}
