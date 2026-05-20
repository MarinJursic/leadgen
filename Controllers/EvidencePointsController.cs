using Leadgen.Model.Entities;
using leadgen.Data;
using leadgen.Services;
using leadgen.ViewModels.Crud;
using leadgen.ViewModels.EvidencePoints;
using leadgen.ViewModels.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace leadgen.Controllers;

// Expose evidence list and details pages.
[Route("evidence")]
public sealed class EvidencePointsController : Controller
{
    // Read-only access to the seeded Leadgen dataset.
    private readonly ILeadgenReadRepository _repository;
    private readonly LeadgenDbContext _dbContext;

    // Receive the repository from dependency injection.
    public EvidencePointsController(ILeadgenReadRepository repository, LeadgenDbContext dbContext)
    {
        _repository = repository;
        _dbContext = dbContext;
    }

    // Show all evidence points ordered by most recently captured first.
    [HttpGet("")]
    public IActionResult Index()
    {
        var evidencePoints = _repository.GetEvidencePoints()
            .OrderByDescending(evidence => evidence.CapturedAtUtc)
            .ToList();

        return View(evidencePoints);
    }

    // Show one evidence point plus the contact, company, and mission above it.
    [HttpGet("{id:guid}")]
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

    [HttpGet("new")]
    public IActionResult Create()
    {
        return View(new EvidencePointFormViewModel
        {
            CapturedAtUtc = DateTime.UtcNow,
            ConfidenceScore = 0.50m
        });
    }

    [HttpPost("new")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(EvidencePointFormViewModel model)
    {
        await ValidateEvidence(model);
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var entity = new EvidencePoint
        {
            Id = Guid.NewGuid(),
            TargetContactId = model.TargetContactId,
            Kind = model.Kind,
            Label = model.Label.Trim(),
            SourcePlatform = model.SourcePlatform.Trim(),
            SourceUrl = model.SourceUrl.Trim(),
            Summary = model.Summary.Trim(),
            RawSnippet = model.RawSnippet.Trim(),
            CapturedAtUtc = NormalizeUtc(model.CapturedAtUtc),
            ConfidenceScore = model.ConfidenceScore,
            IsQualificationSignal = model.IsQualificationSignal
        };

        _dbContext.EvidencePoints.Add(entity);
        await _dbContext.SaveChangesAsync();

        return RedirectToAction(nameof(Details), new { id = entity.Id });
    }

    [HttpGet("{id:guid}/edit")]
    public async Task<IActionResult> Edit(Guid id)
    {
        var evidence = await _dbContext.EvidencePoints.AsNoTracking()
            .Include(item => item.TargetContact)
            .FirstOrDefaultAsync(item => item.Id == id);
        if (evidence is null)
        {
            return NotFound();
        }

        return View(ToForm(evidence));
    }

    [HttpPost("{id:guid}/edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, EvidencePointFormViewModel model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        var evidence = await _dbContext.EvidencePoints.FirstOrDefaultAsync(item => item.Id == id);
        if (evidence is null)
        {
            return NotFound();
        }

        await ValidateEvidence(model);
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        evidence.TargetContactId = model.TargetContactId;
        evidence.Kind = model.Kind;
        evidence.Label = model.Label.Trim();
        evidence.SourcePlatform = model.SourcePlatform.Trim();
        evidence.SourceUrl = model.SourceUrl.Trim();
        evidence.Summary = model.Summary.Trim();
        evidence.RawSnippet = model.RawSnippet.Trim();
        evidence.CapturedAtUtc = NormalizeUtc(model.CapturedAtUtc);
        evidence.ConfidenceScore = model.ConfidenceScore;
        evidence.IsQualificationSignal = model.IsQualificationSignal;

        await _dbContext.SaveChangesAsync();

        return RedirectToAction(nameof(Details), new { id = evidence.Id });
    }

    [HttpGet("{id:guid}/delete")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var evidence = await _dbContext.EvidencePoints.AsNoTracking().FirstOrDefaultAsync(item => item.Id == id);
        if (evidence is null)
        {
            return NotFound();
        }

        return View("~/Views/Shared/DeleteEntity.cshtml", new DeleteEntityViewModel
        {
            Id = evidence.Id,
            EntityName = "Evidence",
            Title = evidence.Label,
            Description = "Deleting an evidence point removes only this captured proof item.",
            ReturnController = "EvidencePoints"
        });
    }

    [HttpPost("{id:guid}/delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id, DeleteEntityViewModel model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        var evidence = await _dbContext.EvidencePoints.FirstOrDefaultAsync(item => item.Id == id);
        if (evidence is null)
        {
            return NotFound();
        }

        _dbContext.EvidencePoints.Remove(evidence);
        await _dbContext.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    private async Task ValidateEvidence(EvidencePointFormViewModel model)
    {
        if (model.TargetContactId == Guid.Empty || !await _dbContext.TargetContacts.AnyAsync(contact => contact.Id == model.TargetContactId))
        {
            ModelState.AddModelError(nameof(model.TargetContactId), "Select an existing contact.");
        }
    }

    private static EvidencePointFormViewModel ToForm(EvidencePoint evidence)
    {
        return new EvidencePointFormViewModel
        {
            Id = evidence.Id,
            TargetContactId = evidence.TargetContactId,
            TargetContactName = evidence.TargetContact?.FullName,
            Kind = evidence.Kind,
            Label = evidence.Label,
            SourcePlatform = evidence.SourcePlatform,
            SourceUrl = evidence.SourceUrl,
            Summary = evidence.Summary,
            RawSnippet = evidence.RawSnippet,
            CapturedAtUtc = evidence.CapturedAtUtc,
            ConfidenceScore = evidence.ConfidenceScore,
            IsQualificationSignal = evidence.IsQualificationSignal
        };
    }

    private static DateTime NormalizeUtc(DateTime value)
    {
        return value.Kind == DateTimeKind.Utc ? value : DateTime.SpecifyKind(value, DateTimeKind.Utc);
    }
}
