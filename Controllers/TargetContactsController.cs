using Leadgen.Model.Entities;
using leadgen.Data;
using leadgen.Services;
using leadgen.ViewModels.Crud;
using leadgen.ViewModels.Shared;
using leadgen.ViewModels.TargetContacts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace leadgen.Controllers;

// Expose target contact list and details pages.
[Authorize]
[Route("contacts")]
public sealed class TargetContactsController : Controller
{
    // Read-only access to the seeded Leadgen dataset.
    private readonly ILeadgenReadRepository _repository;
    private readonly LeadgenDbContext _dbContext;

    // Receive the repository from dependency injection.
    public TargetContactsController(ILeadgenReadRepository repository, LeadgenDbContext dbContext)
    {
        _repository = repository;
        _dbContext = dbContext;
    }

    // Show all contacts with decision-makers first.
    [AllowAnonymous]
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

    [Authorize(Roles = "Admin,Manager")]
    [HttpGet("new")]
    public IActionResult Create()
    {
        return View(new TargetContactFormViewModel
        {
            LastObservedAtUtc = DateTime.UtcNow
        });
    }

    [Authorize(Roles = "Admin,Manager")]
    [HttpPost("new")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TargetContactFormViewModel model)
    {
        await ValidateContact(model);
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var entity = new TargetContact
        {
            Id = Guid.NewGuid(),
            TargetCompanyId = model.TargetCompanyId,
            FullName = model.FullName.Trim(),
            JobTitle = model.JobTitle.Trim(),
            Department = model.Department.Trim(),
            Seniority = model.Seniority.Trim(),
            IsDecisionMaker = model.IsDecisionMaker,
            LinkedInUrl = model.LinkedInUrl?.Trim(),
            XHandle = model.XHandle?.Trim(),
            GitHubUsername = model.GitHubUsername?.Trim(),
            OpportunitySummary = model.OpportunitySummary.Trim(),
            LastObservedAtUtc = NormalizeUtc(model.LastObservedAtUtc)
        };

        _dbContext.TargetContacts.Add(entity);
        await _dbContext.SaveChangesAsync();

        return RedirectToAction(nameof(Details), new { id = entity.Id });
    }

    [Authorize(Roles = "Admin,Manager")]
    [HttpGet("{id:guid}/edit")]
    public async Task<IActionResult> Edit(Guid id)
    {
        var contact = await _dbContext.TargetContacts.AsNoTracking()
            .Include(item => item.TargetCompany)
            .FirstOrDefaultAsync(item => item.Id == id);
        if (contact is null)
        {
            return NotFound();
        }

        return View(ToForm(contact));
    }

    [Authorize(Roles = "Admin,Manager")]
    [HttpPost("{id:guid}/edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, TargetContactFormViewModel model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        var contact = await _dbContext.TargetContacts.FirstOrDefaultAsync(item => item.Id == id);
        if (contact is null)
        {
            return NotFound();
        }

        await ValidateContact(model);
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        contact.TargetCompanyId = model.TargetCompanyId;
        contact.FullName = model.FullName.Trim();
        contact.JobTitle = model.JobTitle.Trim();
        contact.Department = model.Department.Trim();
        contact.Seniority = model.Seniority.Trim();
        contact.IsDecisionMaker = model.IsDecisionMaker;
        contact.LinkedInUrl = model.LinkedInUrl?.Trim();
        contact.XHandle = model.XHandle?.Trim();
        contact.GitHubUsername = model.GitHubUsername?.Trim();
        contact.OpportunitySummary = model.OpportunitySummary.Trim();
        contact.LastObservedAtUtc = NormalizeUtc(model.LastObservedAtUtc);

        await _dbContext.SaveChangesAsync();

        return RedirectToAction(nameof(Details), new { id = contact.Id });
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("{id:guid}/delete")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var contact = await _dbContext.TargetContacts.AsNoTracking()
            .Include(item => item.ContactChannels)
            .Include(item => item.EvidencePoints)
            .Include(item => item.LeadDossiers)
            .FirstOrDefaultAsync(item => item.Id == id);
        if (contact is null)
        {
            return NotFound();
        }

        return View("~/Views/Shared/DeleteEntity.cshtml", new DeleteEntityViewModel
        {
            Id = contact.Id,
            EntityName = "Contacts",
            Title = contact.FullName,
            Description = "Deleting a contact removes its channels, evidence, and linked dossiers.",
            ReturnController = "TargetContacts",
            Warnings =
            [
                $"{contact.ContactChannels.Count} channel(s) will be removed.",
                $"{contact.EvidencePoints.Count} evidence point(s) will be removed.",
                $"{contact.LeadDossiers.Count} dossier(s) will be removed."
            ]
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

        var contact = await _dbContext.TargetContacts.FirstOrDefaultAsync(item => item.Id == id);
        if (contact is null)
        {
            return NotFound();
        }

        var dossiers = await _dbContext.LeadDossiers.Where(dossier => dossier.TargetContactId == id).ToListAsync();
        _dbContext.LeadDossiers.RemoveRange(dossiers);
        _dbContext.TargetContacts.Remove(contact);
        await _dbContext.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    private async Task ValidateContact(TargetContactFormViewModel model)
    {
        if (model.TargetCompanyId == Guid.Empty || !await _dbContext.TargetCompanies.AnyAsync(company => company.Id == model.TargetCompanyId))
        {
            ModelState.AddModelError(nameof(model.TargetCompanyId), "Select an existing company.");
        }
    }

    private static TargetContactFormViewModel ToForm(TargetContact contact)
    {
        return new TargetContactFormViewModel
        {
            Id = contact.Id,
            TargetCompanyId = contact.TargetCompanyId,
            TargetCompanyName = contact.TargetCompany?.Name,
            FullName = contact.FullName,
            JobTitle = contact.JobTitle,
            Department = contact.Department,
            Seniority = contact.Seniority,
            IsDecisionMaker = contact.IsDecisionMaker,
            LinkedInUrl = contact.LinkedInUrl,
            XHandle = contact.XHandle,
            GitHubUsername = contact.GitHubUsername,
            OpportunitySummary = contact.OpportunitySummary,
            LastObservedAtUtc = contact.LastObservedAtUtc
        };
    }

    private static DateTime NormalizeUtc(DateTime value)
    {
        return value.Kind == DateTimeKind.Utc ? value : DateTime.SpecifyKind(value, DateTimeKind.Utc);
    }
}
