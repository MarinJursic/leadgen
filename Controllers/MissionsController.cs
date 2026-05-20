using Leadgen.Model.Entities;
using leadgen.Data;
using leadgen.Services;
using leadgen.ViewModels.Crud;
using leadgen.ViewModels.Missions;
using leadgen.ViewModels.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace leadgen.Controllers;

// Expose mission list and mission details pages.
[Route("missions")]
public sealed class MissionsController : Controller
{
    // Read-only access to the seeded Leadgen dataset.
    private readonly ILeadgenReadRepository _repository;
    private readonly LeadgenDbContext _dbContext;

    // Receive the repository from dependency injection.
    public MissionsController(ILeadgenReadRepository repository, LeadgenDbContext dbContext)
    {
        _repository = repository;
        _dbContext = dbContext;
    }

    // Show all missions ordered by highest confidence first.
    [HttpGet("")]
    public IActionResult Index()
    {
        // Load and sort the missions for the index table.
        var missions = _repository.GetMissions()
            .OrderByDescending(mission => mission.ConfidenceScore)
            .ToList();

        return View(missions);
    }

    // Show one mission plus its latest run and aggregate output counts.
    [HttpGet("{id:guid}")]
    public IActionResult Details(Guid id)
    {
        // Find the requested mission by id.
        var mission = _repository.GetMission(id);
        if (mission is null)
        {
            return NotFound();
        }

        // Pick the newest run so the details page can summarize the latest execution.
        var latestRun = mission.Runs
            .OrderByDescending(run => run.StartedAtUtc)
            .FirstOrDefault();

        // Build a UI-specific view model with derived counts.
        var model = new MissionDetailsViewModel
        {
            Mission = mission,
            LatestRun = latestRun,
            CompanyCount = mission.Runs.SelectMany(run => run.TargetCompanies).Count(),
            ContactCount = mission.Runs.SelectMany(run => run.TargetCompanies).SelectMany(company => company.Contacts).Count(),
            DossierCount = mission.Runs.SelectMany(run => run.LeadDossiers).Count()
        };

        return View(model);
    }

    [HttpGet("new")]
    public IActionResult Create()
    {
        return View(new MissionFormViewModel
        {
            CreatedAtUtc = DateTime.UtcNow,
            ConfidenceScore = 0.50m
        });
    }

    [HttpPost("new")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(MissionFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var entity = new BusinessDnaMission
        {
            Id = Guid.NewGuid(),
            MissionName = model.MissionName.Trim(),
            ProductName = model.ProductName.Trim(),
            Mechanic = model.Mechanic.Trim(),
            PrimarySurface = model.PrimarySurface.Trim(),
            SurfaceTags = ParseTags(model.SurfaceTagsText),
            Persona = model.Persona.Trim(),
            Villain = model.Villain.Trim(),
            Delta = model.Delta.Trim(),
            ConfidenceScore = model.ConfidenceScore,
            CreatedAtUtc = NormalizeUtc(model.CreatedAtUtc),
            Status = model.Status
        };

        _dbContext.BusinessDnaMissions.Add(entity);
        await _dbContext.SaveChangesAsync();

        return RedirectToAction(nameof(Details), new { id = entity.Id });
    }

    [HttpGet("{id:guid}/edit")]
    public async Task<IActionResult> Edit(Guid id)
    {
        var mission = await _dbContext.BusinessDnaMissions.AsNoTracking().FirstOrDefaultAsync(item => item.Id == id);
        if (mission is null)
        {
            return NotFound();
        }

        return View(ToForm(mission));
    }

    [HttpPost("{id:guid}/edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, MissionFormViewModel model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        var mission = await _dbContext.BusinessDnaMissions.FirstOrDefaultAsync(item => item.Id == id);
        if (mission is null)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        mission.MissionName = model.MissionName.Trim();
        mission.ProductName = model.ProductName.Trim();
        mission.Mechanic = model.Mechanic.Trim();
        mission.PrimarySurface = model.PrimarySurface.Trim();
        mission.SurfaceTags = ParseTags(model.SurfaceTagsText);
        mission.Persona = model.Persona.Trim();
        mission.Villain = model.Villain.Trim();
        mission.Delta = model.Delta.Trim();
        mission.ConfidenceScore = model.ConfidenceScore;
        mission.CreatedAtUtc = NormalizeUtc(model.CreatedAtUtc);
        mission.Status = model.Status;

        await _dbContext.SaveChangesAsync();

        return RedirectToAction(nameof(Details), new { id = mission.Id });
    }

    [HttpGet("{id:guid}/delete")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var mission = await _dbContext.BusinessDnaMissions.AsNoTracking()
            .Include(item => item.Runs)
            .Include(item => item.ClarificationQuestions)
            .FirstOrDefaultAsync(item => item.Id == id);
        if (mission is null)
        {
            return NotFound();
        }

        return View("~/Views/Shared/DeleteEntity.cshtml", new DeleteEntityViewModel
        {
            Id = mission.Id,
            EntityName = "Missions",
            Title = mission.MissionName,
            Description = "Deleting a mission removes its clarification questions, runs, assignments, companies, contacts, evidence, channels, and dossiers.",
            ReturnController = "Missions",
            Warnings =
            [
                $"{mission.ClarificationQuestions.Count} clarification question(s) will be removed.",
                $"{mission.Runs.Count} run(s) and their outputs will be removed."
            ]
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

        var mission = await _dbContext.BusinessDnaMissions
            .Include(item => item.Runs)
            .FirstOrDefaultAsync(item => item.Id == id);
        if (mission is null)
        {
            return NotFound();
        }

        var runIds = mission.Runs.Select(run => run.Id).ToList();
        var dossiers = await _dbContext.LeadDossiers.Where(dossier => runIds.Contains(dossier.MissionRunId)).ToListAsync();
        _dbContext.LeadDossiers.RemoveRange(dossiers);
        _dbContext.BusinessDnaMissions.Remove(mission);
        await _dbContext.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    private static MissionFormViewModel ToForm(BusinessDnaMission mission)
    {
        return new MissionFormViewModel
        {
            Id = mission.Id,
            MissionName = mission.MissionName,
            ProductName = mission.ProductName,
            Mechanic = mission.Mechanic,
            PrimarySurface = mission.PrimarySurface,
            SurfaceTagsText = string.Join(", ", mission.SurfaceTags),
            Persona = mission.Persona,
            Villain = mission.Villain,
            Delta = mission.Delta,
            ConfidenceScore = mission.ConfidenceScore,
            CreatedAtUtc = mission.CreatedAtUtc,
            Status = mission.Status
        };
    }

    private static List<string> ParseTags(string? tags)
    {
        return (tags ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static DateTime NormalizeUtc(DateTime value)
    {
        return value.Kind == DateTimeKind.Utc ? value : DateTime.SpecifyKind(value, DateTimeKind.Utc);
    }
}
