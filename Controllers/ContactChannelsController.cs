using leadgen.Services;
using leadgen.ViewModels.ContactChannels;
using Microsoft.AspNetCore.Mvc;

namespace leadgen.Controllers;

public sealed class ContactChannelsController : Controller
{
    private readonly ILeadgenReadRepository _repository;

    public ContactChannelsController(ILeadgenReadRepository repository)
    {
        _repository = repository;
    }

    public IActionResult Index()
    {
        var channels = _repository.GetContactChannels()
            .OrderByDescending(channel => channel.IsVerified)
            .ThenBy(channel => channel.Type)
            .ToList();

        return View(channels);
    }

    public IActionResult Details(Guid id)
    {
        var channel = _repository.GetContactChannel(id);
        if (channel is null)
        {
            return NotFound();
        }

        var contact = _repository.GetTargetContacts().FirstOrDefault(item => item.ContactChannels.Any(candidate => candidate.Id == id));
        var company = _repository.GetTargetCompanies().FirstOrDefault(item => item.Contacts.Any(contactItem => contactItem.ContactChannels.Any(candidate => candidate.Id == id)));

        return View(new ContactChannelDetailsViewModel
        {
            Channel = channel,
            Contact = contact,
            Company = company
        });
    }
}
