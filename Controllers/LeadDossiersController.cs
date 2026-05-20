using Leadgen.Model.Entities;
using leadgen.Data;
using leadgen.Services;
using leadgen.ViewModels.Crud;
using leadgen.ViewModels.LeadDossiers;
using leadgen.ViewModels.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace leadgen.Controllers;

// Expose final lead dossier list and details pages.
[Route("dossiers")]
public sealed class LeadDossiersController : Controller
{
    // Read-only access to the seeded Leadgen dataset.
    private readonly ILeadgenReadRepository _repository;
    private readonly LeadgenDbContext _dbContext;

    // Receive the repository from dependency injection.
    public LeadDossiersController(ILeadgenReadRepository repository, LeadgenDbContext dbContext)
    {
        _repository = repository;
        _dbContext = dbContext;
    }

    // Show all dossiers ordered by highest score, then most recently updated.
    [HttpGet("")]
    public IActionResult Index()
    {
        var dossiers = _repository.GetLeadDossiers()
            .OrderByDescending(dossier => dossier.LeadgenScore)
            .ThenByDescending(dossier => dossier.LastUpdatedAtUtc)
            .ToList();

        return View(dossiers);
    }

    // Show one dossier plus its run, mission, company, and contact context.
    [HttpGet("{id:guid}")]
    public IActionResult Details(Guid id)
    {
        var dossier = _repository.GetLeadDossier(id);
        if (dossier is null)
        {
            return NotFound();
        }

        // Resolve the surrounding context using the dossier's linked ids.
        var run = _repository.GetMissionRun(dossier.MissionRunId);
        var mission = _repository.GetMissions().FirstOrDefault(item => item.Runs.Any(candidate => candidate.Id == dossier.MissionRunId));
        var company = _repository.GetTargetCompany(dossier.TargetCompanyId);
        var contact = _repository.GetTargetContact(dossier.TargetContactId);

        // Send the assembled details model to the view.
        return View(new LeadDossierDetailsViewModel
        {
            Dossier = dossier,
            Run = run,
            Mission = mission,
            Company = company,
            Contact = contact
        });
    }

    [HttpGet("new")]
    public IActionResult Create()
    {
        return View(new LeadDossierFormViewModel
        {
            CreatedAtUtc = DateTime.UtcNow,
            LastUpdatedAtUtc = DateTime.UtcNow,
            LeadgenScore = 50
        });
    }

    [HttpPost("new")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(LeadDossierFormViewModel model)
    {
        await ValidateDossier(model);
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var entity = new LeadDossier
        {
            Id = Guid.NewGuid(),
            MissionRunId = model.MissionRunId,
            TargetCompanyId = model.TargetCompanyId,
            TargetContactId = model.TargetContactId,
            LeadgenScore = model.LeadgenScore,
            SuggestedApproach = model.SuggestedApproach.Trim(),
            AdvantagePoint = model.AdvantagePoint.Trim(),
            IsReadyForOutreach = model.IsReadyForOutreach,
            CreatedAtUtc = NormalizeUtc(model.CreatedAtUtc),
            LastUpdatedAtUtc = NormalizeUtc(model.LastUpdatedAtUtc),
            SupportingEvidenceCount = model.SupportingEvidenceCount
        };

        _dbContext.LeadDossiers.Add(entity);
        await _dbContext.SaveChangesAsync();

        return RedirectToAction(nameof(Details), new { id = entity.Id });
    }

    [HttpGet("{id:guid}/edit")]
    public async Task<IActionResult> Edit(Guid id)
    {
        var dossier = await _dbContext.LeadDossiers.AsNoTracking()
            .Include(item => item.MissionRun)
            .Include(item => item.TargetCompany)
            .Include(item => item.TargetContact)
            .FirstOrDefaultAsync(item => item.Id == id);
        if (dossier is null)
        {
            return NotFound();
        }

        return View(ToForm(dossier));
    }

    [HttpPost("{id:guid}/edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, LeadDossierFormViewModel model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        var dossier = await _dbContext.LeadDossiers.FirstOrDefaultAsync(item => item.Id == id);
        if (dossier is null)
        {
            return NotFound();
        }

        await ValidateDossier(model);
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        dossier.MissionRunId = model.MissionRunId;
        dossier.TargetCompanyId = model.TargetCompanyId;
        dossier.TargetContactId = model.TargetContactId;
        dossier.LeadgenScore = model.LeadgenScore;
        dossier.SuggestedApproach = model.SuggestedApproach.Trim();
        dossier.AdvantagePoint = model.AdvantagePoint.Trim();
        dossier.IsReadyForOutreach = model.IsReadyForOutreach;
        dossier.CreatedAtUtc = NormalizeUtc(model.CreatedAtUtc);
        dossier.LastUpdatedAtUtc = NormalizeUtc(model.LastUpdatedAtUtc);
        dossier.SupportingEvidenceCount = model.SupportingEvidenceCount;

        await _dbContext.SaveChangesAsync();

        return RedirectToAction(nameof(Details), new { id = dossier.Id });
    }

    [HttpGet("{id:guid}/delete")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var dossier = await _dbContext.LeadDossiers.AsNoTracking().FirstOrDefaultAsync(item => item.Id == id);
        if (dossier is null)
        {
            return NotFound();
        }

        return View("~/Views/Shared/DeleteEntity.cshtml", new DeleteEntityViewModel
        {
            Id = dossier.Id,
            EntityName = "Dossiers",
            Title = $"Lead score {dossier.LeadgenScore}",
            Description = "Deleting a dossier removes only this final outreach recommendation.",
            ReturnController = "LeadDossiers"
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

        var dossier = await _dbContext.LeadDossiers.FirstOrDefaultAsync(item => item.Id == id);
        if (dossier is null)
        {
            return NotFound();
        }

        _dbContext.LeadDossiers.Remove(dossier);
        await _dbContext.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    private async Task ValidateDossier(LeadDossierFormViewModel model)
    {
        if (model.MissionRunId == Guid.Empty || !await _dbContext.MissionRuns.AnyAsync(run => run.Id == model.MissionRunId))
        {
            ModelState.AddModelError(nameof(model.MissionRunId), "Select an existing run.");
        }

        if (model.TargetCompanyId == Guid.Empty || !await _dbContext.TargetCompanies.AnyAsync(company => company.Id == model.TargetCompanyId))
        {
            ModelState.AddModelError(nameof(model.TargetCompanyId), "Select an existing company.");
        }

        var contact = await _dbContext.TargetContacts.AsNoTracking().FirstOrDefaultAsync(item => item.Id == model.TargetContactId);
        if (model.TargetContactId == Guid.Empty || contact is null)
        {
            ModelState.AddModelError(nameof(model.TargetContactId), "Select an existing contact.");
        }
        else if (contact.TargetCompanyId != model.TargetCompanyId)
        {
            ModelState.AddModelError(nameof(model.TargetContactId), "Selected contact must belong to the selected company.");
        }

        if (model.LastUpdatedAtUtc < model.CreatedAtUtc)
        {
            ModelState.AddModelError(nameof(model.LastUpdatedAtUtc), "Last updated time cannot be before created time.");
        }
    }

    private static LeadDossierFormViewModel ToForm(LeadDossier dossier)
    {
        return new LeadDossierFormViewModel
        {
            Id = dossier.Id,
            MissionRunId = dossier.MissionRunId,
            MissionRunName = dossier.MissionRun?.RunCode,
            TargetCompanyId = dossier.TargetCompanyId,
            TargetCompanyName = dossier.TargetCompany?.Name,
            TargetContactId = dossier.TargetContactId,
            TargetContactName = dossier.TargetContact?.FullName,
            LeadgenScore = dossier.LeadgenScore,
            SuggestedApproach = dossier.SuggestedApproach,
            AdvantagePoint = dossier.AdvantagePoint,
            IsReadyForOutreach = dossier.IsReadyForOutreach,
            CreatedAtUtc = dossier.CreatedAtUtc,
            LastUpdatedAtUtc = dossier.LastUpdatedAtUtc,
            SupportingEvidenceCount = dossier.SupportingEvidenceCount
        };
    }

    private static DateTime NormalizeUtc(DateTime value)
    {
        return value.Kind == DateTimeKind.Utc ? value : DateTime.SpecifyKind(value, DateTimeKind.Utc);
    }
}
