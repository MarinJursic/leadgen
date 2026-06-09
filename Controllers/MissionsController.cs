using Leadgen.Model.Entities;
using leadgen.Data;
using leadgen.Services;
using leadgen.ViewModels.Crud;
using leadgen.ViewModels.Missions;
using leadgen.ViewModels.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace leadgen.Controllers;

// Expose mission list and mission details pages.
[Authorize]
[Route("missions")]
public sealed class MissionsController : Controller
{
    // Read-only access to the seeded Leadgen dataset.
    private readonly ILeadgenReadRepository _repository;
    private readonly LeadgenDbContext _dbContext;
    private readonly IWebHostEnvironment _environment;

    // Receive the repository from dependency injection.
    public MissionsController(ILeadgenReadRepository repository, LeadgenDbContext dbContext, IWebHostEnvironment environment)
    {
        _repository = repository;
        _dbContext = dbContext;
        _environment = environment;
    }

    // Show all missions ordered by highest confidence first.
    [AllowAnonymous]
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

    [Authorize(Roles = "Admin,Manager")]
    [HttpGet("new")]
    public IActionResult Create()
    {
        return View(new MissionFormViewModel
        {
            CreatedAtUtc = DateTime.UtcNow,
            ConfidenceScore = 0.50m
        });
    }

    [Authorize(Roles = "Admin,Manager")]
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

    [Authorize(Roles = "Admin,Manager")]
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

    [Authorize(Roles = "Admin,Manager")]
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

    [Authorize(Roles = "Admin")]
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

    [Authorize(Roles = "Admin")]
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

    [Authorize(Roles = "Admin,Manager")]
    [HttpGet("{missionId:guid}/attachments")]
    public async Task<IActionResult> GetAttachments(Guid missionId)
    {
        if (!await _dbContext.BusinessDnaMissions.AnyAsync(mission => mission.Id == missionId))
        {
            return NotFound();
        }

        var attachments = await _dbContext.MissionAttachments.AsNoTracking()
            .Where(attachment => attachment.BusinessDnaMissionId == missionId)
            .OrderByDescending(attachment => attachment.CreatedAtUtc)
            .ToListAsync();

        return PartialView("_AttachmentList", attachments);
    }

    [Authorize(Roles = "Admin,Manager")]
    [HttpPost("{missionId:guid}/attachments")]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(10_000_000)]
    public async Task<IActionResult> UploadAttachment(Guid missionId, IFormFile? file)
    {
        var missionExists = await _dbContext.BusinessDnaMissions.AnyAsync(mission => mission.Id == missionId);
        if (!missionExists)
        {
            return NotFound();
        }

        if (file is null || file.Length == 0)
        {
            return BadRequest(new { message = "A non-empty file is required." });
        }

        if (file.Length > 10_000_000)
        {
            return BadRequest(new { message = "The file is too large." });
        }

        var originalFileName = Path.GetFileName(file.FileName);
        var extension = Path.GetExtension(originalFileName).ToLowerInvariant();
        var allowedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".csv",
            ".doc",
            ".docx",
            ".jpeg",
            ".jpg",
            ".json",
            ".md",
            ".pdf",
            ".png",
            ".txt"
        };
        if (!allowedExtensions.Contains(extension))
        {
            return BadRequest(new { message = "This file type is not allowed." });
        }

        var uploadsPath = Path.Combine(WebRootPath, "uploads", "missions", missionId.ToString());
        Directory.CreateDirectory(uploadsPath);

        var storageFileName = $"{Guid.NewGuid():N}{extension}";
        var physicalPath = Path.Combine(uploadsPath, storageFileName);
        await using (var stream = System.IO.File.Create(physicalPath))
        {
            await file.CopyToAsync(stream);
        }

        var attachment = new MissionAttachment
        {
            Id = Guid.NewGuid(),
            BusinessDnaMissionId = missionId,
            FileName = originalFileName,
            StorageFileName = storageFileName,
            FilePath = $"/uploads/missions/{missionId}/{storageFileName}",
            ContentType = string.IsNullOrWhiteSpace(file.ContentType) ? "application/octet-stream" : file.ContentType,
            FileSize = file.Length,
            CreatedAtUtc = DateTime.UtcNow,
            UploadedByUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)
        };

        _dbContext.MissionAttachments.Add(attachment);
        await _dbContext.SaveChangesAsync();

        return Json(new { success = true, attachment.Id });
    }

    [Authorize(Roles = "Admin,Manager")]
    [HttpPost("attachments/{id:guid}/delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAttachment(Guid id)
    {
        var attachment = await _dbContext.MissionAttachments.FirstOrDefaultAsync(item => item.Id == id);
        if (attachment is null)
        {
            return NotFound();
        }

        var physicalPath = ToPhysicalAttachmentPath(attachment.FilePath);
        if (System.IO.File.Exists(physicalPath))
        {
            System.IO.File.Delete(physicalPath);
        }

        _dbContext.MissionAttachments.Remove(attachment);
        await _dbContext.SaveChangesAsync();

        return Json(new { success = true });
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

    private string WebRootPath => _environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot");

    private string ToPhysicalAttachmentPath(string relativePath)
    {
        var safeRelativePath = relativePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        return Path.Combine(WebRootPath, safeRelativePath);
    }
}
