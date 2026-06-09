using Leadgen.Model.Entities;
using leadgen.Data;
using leadgen.Services;
using leadgen.ViewModels.Crud;
using leadgen.ViewModels.MissionRuns;
using leadgen.ViewModels.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace leadgen.Controllers;

// Expose mission run list and details pages.
[Authorize]
[Route("runs")]
public sealed class MissionRunsController : Controller
{
    // Read-only access to the seeded Leadgen dataset.
    private readonly ILeadgenReadRepository _repository;
    private readonly LeadgenDbContext _dbContext;

    // Receive the repository from dependency injection.
    public MissionRunsController(ILeadgenReadRepository repository, LeadgenDbContext dbContext)
    {
        _repository = repository;
        _dbContext = dbContext;
    }

    // Show all mission runs ordered by most recent start time.
    [AllowAnonymous]
    [HttpGet("")]
    public IActionResult Index()
    {
        var runs = _repository.GetMissionRuns()
            .OrderByDescending(run => run.StartedAtUtc)
            .ToList();

        return View(runs);
    }

    // Show one mission run and its related mission, assignments, companies, and dossiers.
    [HttpGet("{id:guid}")]
    public IActionResult Details(Guid id)
    {
        var run = _repository.GetMissionRun(id);
        if (run is null)
        {
            return NotFound();
        }

        // Find the mission that contains this run.
        var mission = _repository.GetMissions().FirstOrDefault(item => item.Runs.Any(candidate => candidate.Id == id));

        // Package the run and its outputs into a UI-specific view model.
        return View(new MissionRunDetailsViewModel
        {
            Run = run,
            Mission = mission,
            Assignments = run.AgentAssignments.ToList(),
            Companies = run.TargetCompanies.ToList(),
            Dossiers = run.LeadDossiers.ToList()
        });
    }

    [Authorize(Roles = "Admin,Manager")]
    [HttpGet("new")]
    public IActionResult Create()
    {
        return View(new MissionRunFormViewModel
        {
            StartedAtUtc = DateTime.UtcNow,
            Status = Leadgen.Model.Enums.MissionStatus.Running,
            TokenBudget = 1000
        });
    }

    [Authorize(Roles = "Admin,Manager")]
    [HttpPost("new")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(MissionRunFormViewModel model)
    {
        await ValidateMissionRun(model);
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var entity = new MissionRun
        {
            Id = Guid.NewGuid(),
            RunCode = model.RunCode.Trim(),
            BusinessDnaMissionId = model.BusinessDnaMissionId,
            StartedAtUtc = NormalizeUtc(model.StartedAtUtc),
            CompletedAtUtc = model.CompletedAtUtc.HasValue ? NormalizeUtc(model.CompletedAtUtc.Value) : null,
            Status = model.Status,
            SearchRegion = model.SearchRegion.Trim(),
            TokenBudget = model.TokenBudget,
            EstimatedCostUsd = model.EstimatedCostUsd
        };

        _dbContext.MissionRuns.Add(entity);
        await _dbContext.SaveChangesAsync();

        return RedirectToAction(nameof(Details), new { id = entity.Id });
    }

    [Authorize(Roles = "Admin,Manager")]
    [HttpGet("{id:guid}/edit")]
    public async Task<IActionResult> Edit(Guid id)
    {
        var run = await _dbContext.MissionRuns.AsNoTracking()
            .Include(item => item.Mission)
            .FirstOrDefaultAsync(item => item.Id == id);
        if (run is null)
        {
            return NotFound();
        }

        return View(ToForm(run));
    }

    [Authorize(Roles = "Admin,Manager")]
    [HttpPost("{id:guid}/edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, MissionRunFormViewModel model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        var run = await _dbContext.MissionRuns.FirstOrDefaultAsync(item => item.Id == id);
        if (run is null)
        {
            return NotFound();
        }

        await ValidateMissionRun(model, id);
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        run.RunCode = model.RunCode.Trim();
        run.BusinessDnaMissionId = model.BusinessDnaMissionId;
        run.StartedAtUtc = NormalizeUtc(model.StartedAtUtc);
        run.CompletedAtUtc = model.CompletedAtUtc.HasValue ? NormalizeUtc(model.CompletedAtUtc.Value) : null;
        run.Status = model.Status;
        run.SearchRegion = model.SearchRegion.Trim();
        run.TokenBudget = model.TokenBudget;
        run.EstimatedCostUsd = model.EstimatedCostUsd;

        await _dbContext.SaveChangesAsync();

        return RedirectToAction(nameof(Details), new { id = run.Id });
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("{id:guid}/delete")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var run = await _dbContext.MissionRuns.AsNoTracking()
            .Include(item => item.AgentAssignments)
            .Include(item => item.TargetCompanies)
            .Include(item => item.LeadDossiers)
            .FirstOrDefaultAsync(item => item.Id == id);
        if (run is null)
        {
            return NotFound();
        }

        return View("~/Views/Shared/DeleteEntity.cshtml", new DeleteEntityViewModel
        {
            Id = run.Id,
            EntityName = "Runs",
            Title = run.RunCode,
            Description = "Deleting a run removes its assignments, companies, contacts, evidence, channels, and dossiers.",
            ReturnController = "MissionRuns",
            Warnings =
            [
                $"{run.AgentAssignments.Count} assignment(s) will be removed.",
                $"{run.TargetCompanies.Count} company record(s) will be removed.",
                $"{run.LeadDossiers.Count} dossier(s) will be removed."
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

        var run = await _dbContext.MissionRuns.FirstOrDefaultAsync(item => item.Id == id);
        if (run is null)
        {
            return NotFound();
        }

        var dossiers = await _dbContext.LeadDossiers.Where(dossier => dossier.MissionRunId == id).ToListAsync();
        _dbContext.LeadDossiers.RemoveRange(dossiers);
        _dbContext.MissionRuns.Remove(run);
        await _dbContext.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    private async Task ValidateMissionRun(MissionRunFormViewModel model, Guid? currentId = null)
    {
        if (model.BusinessDnaMissionId == Guid.Empty
            || !await _dbContext.BusinessDnaMissions.AnyAsync(mission => mission.Id == model.BusinessDnaMissionId))
        {
            ModelState.AddModelError(nameof(model.BusinessDnaMissionId), "Select an existing mission.");
        }

        if (model.CompletedAtUtc.HasValue && model.CompletedAtUtc.Value < model.StartedAtUtc)
        {
            ModelState.AddModelError(nameof(model.CompletedAtUtc), "Completed time cannot be before started time.");
        }

        if (await _dbContext.MissionRuns.AnyAsync(run => run.RunCode == model.RunCode.Trim() && run.Id != currentId))
        {
            ModelState.AddModelError(nameof(model.RunCode), "Run code must be unique.");
        }
    }

    private static MissionRunFormViewModel ToForm(MissionRun run)
    {
        return new MissionRunFormViewModel
        {
            Id = run.Id,
            RunCode = run.RunCode,
            BusinessDnaMissionId = run.BusinessDnaMissionId,
            BusinessDnaMissionName = run.Mission?.MissionName,
            StartedAtUtc = run.StartedAtUtc,
            CompletedAtUtc = run.CompletedAtUtc,
            Status = run.Status,
            SearchRegion = run.SearchRegion,
            TokenBudget = run.TokenBudget,
            EstimatedCostUsd = run.EstimatedCostUsd
        };
    }

    private static DateTime NormalizeUtc(DateTime value)
    {
        return value.Kind == DateTimeKind.Utc ? value : DateTime.SpecifyKind(value, DateTimeKind.Utc);
    }
}
