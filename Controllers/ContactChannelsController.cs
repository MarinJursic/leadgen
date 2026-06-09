using Leadgen.Model.Entities;
using leadgen.Data;
using leadgen.Services;
using leadgen.ViewModels.ContactChannels;
using leadgen.ViewModels.Crud;
using leadgen.ViewModels.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace leadgen.Controllers;

// Expose contact channel list and details pages.
[Authorize]
[Route("channels")]
public sealed class ContactChannelsController : Controller
{
    // Read-only access to the seeded Leadgen dataset.
    private readonly ILeadgenReadRepository _repository;
    private readonly LeadgenDbContext _dbContext;

    // Receive the repository from dependency injection.
    public ContactChannelsController(ILeadgenReadRepository repository, LeadgenDbContext dbContext)
    {
        _repository = repository;
        _dbContext = dbContext;
    }

    // Show all contact channels with verified channels first.
    [AllowAnonymous]
    [HttpGet("")]
    public IActionResult Index()
    {
        var channels = _repository.GetContactChannels()
            .OrderByDescending(channel => channel.IsVerified)
            .ThenBy(channel => channel.Type)
            .ToList();

        return View(channels);
    }

    // Show one contact channel and the contact/company that owns it.
    [HttpGet("{id:guid}")]
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

    [Authorize(Roles = "Admin,Manager")]
    [HttpGet("new")]
    public IActionResult Create()
    {
        return View(new ContactChannelFormViewModel
        {
            ConfidenceScore = 0.50m
        });
    }

    [Authorize(Roles = "Admin,Manager")]
    [HttpPost("new")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ContactChannelFormViewModel model)
    {
        await ValidateChannel(model);
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var entity = new ContactChannel
        {
            Id = Guid.NewGuid(),
            TargetContactId = model.TargetContactId,
            Type = model.Type,
            Value = model.Value.Trim(),
            IsVerified = model.IsVerified,
            VerifiedAtUtc = model.VerifiedAtUtc.HasValue ? NormalizeUtc(model.VerifiedAtUtc.Value) : null,
            Source = model.Source.Trim(),
            ConfidenceScore = model.ConfidenceScore
        };

        _dbContext.ContactChannels.Add(entity);
        await _dbContext.SaveChangesAsync();

        return RedirectToAction(nameof(Details), new { id = entity.Id });
    }

    [Authorize(Roles = "Admin,Manager")]
    [HttpGet("{id:guid}/edit")]
    public async Task<IActionResult> Edit(Guid id)
    {
        var channel = await _dbContext.ContactChannels.AsNoTracking()
            .Include(item => item.TargetContact)
            .FirstOrDefaultAsync(item => item.Id == id);
        if (channel is null)
        {
            return NotFound();
        }

        return View(ToForm(channel));
    }

    [Authorize(Roles = "Admin,Manager")]
    [HttpPost("{id:guid}/edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, ContactChannelFormViewModel model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        var channel = await _dbContext.ContactChannels.FirstOrDefaultAsync(item => item.Id == id);
        if (channel is null)
        {
            return NotFound();
        }

        await ValidateChannel(model);
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        channel.TargetContactId = model.TargetContactId;
        channel.Type = model.Type;
        channel.Value = model.Value.Trim();
        channel.IsVerified = model.IsVerified;
        channel.VerifiedAtUtc = model.VerifiedAtUtc.HasValue ? NormalizeUtc(model.VerifiedAtUtc.Value) : null;
        channel.Source = model.Source.Trim();
        channel.ConfidenceScore = model.ConfidenceScore;

        await _dbContext.SaveChangesAsync();

        return RedirectToAction(nameof(Details), new { id = channel.Id });
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("{id:guid}/delete")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var channel = await _dbContext.ContactChannels.AsNoTracking().FirstOrDefaultAsync(item => item.Id == id);
        if (channel is null)
        {
            return NotFound();
        }

        return View("~/Views/Shared/DeleteEntity.cshtml", new DeleteEntityViewModel
        {
            Id = channel.Id,
            EntityName = "Channels",
            Title = $"{channel.Type}: {channel.Value}",
            Description = "Deleting a channel removes only this contact method.",
            ReturnController = "ContactChannels"
        });
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("{id:guid}/delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id, DeleteEntityViewModel model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        var channel = await _dbContext.ContactChannels.FirstOrDefaultAsync(item => item.Id == id);
        if (channel is null)
        {
            return NotFound();
        }

        _dbContext.ContactChannels.Remove(channel);
        await _dbContext.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    private async Task ValidateChannel(ContactChannelFormViewModel model)
    {
        if (model.TargetContactId == Guid.Empty || !await _dbContext.TargetContacts.AnyAsync(contact => contact.Id == model.TargetContactId))
        {
            ModelState.AddModelError(nameof(model.TargetContactId), "Select an existing contact.");
        }

        if (model.IsVerified && !model.VerifiedAtUtc.HasValue)
        {
            ModelState.AddModelError(nameof(model.VerifiedAtUtc), "Verified channels need a verification time.");
        }
    }

    private static ContactChannelFormViewModel ToForm(ContactChannel channel)
    {
        return new ContactChannelFormViewModel
        {
            Id = channel.Id,
            TargetContactId = channel.TargetContactId,
            TargetContactName = channel.TargetContact?.FullName,
            Type = channel.Type,
            Value = channel.Value,
            IsVerified = channel.IsVerified,
            VerifiedAtUtc = channel.VerifiedAtUtc,
            Source = channel.Source,
            ConfidenceScore = channel.ConfidenceScore
        };
    }

    private static DateTime NormalizeUtc(DateTime value)
    {
        return value.Kind == DateTimeKind.Utc ? value : DateTime.SpecifyKind(value, DateTimeKind.Utc);
    }
}
