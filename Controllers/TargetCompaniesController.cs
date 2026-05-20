using Leadgen.Model.Entities;
using leadgen.Data;
using leadgen.Services;
using leadgen.ViewModels.Crud;
using leadgen.ViewModels.Shared;
using leadgen.ViewModels.TargetCompanies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace leadgen.Controllers;

// Expose target company list and details pages.
[Route("companies")]
public sealed class TargetCompaniesController : Controller
{
    // Read-only access to the seeded Leadgen dataset.
    private readonly ILeadgenReadRepository _repository;
    private readonly LeadgenDbContext _dbContext;

    // Receive the repository from dependency injection.
    public TargetCompaniesController(ILeadgenReadRepository repository, LeadgenDbContext dbContext)
    {
        _repository = repository;
        _dbContext = dbContext;
    }

    // Show all target companies ordered by best match score first.
    [HttpGet("")]
    public IActionResult Index()
    {
        var companies = _repository.GetTargetCompanies()
            .OrderByDescending(company => company.MatchScore)
            .ToList();

        return View(companies);
    }

    // Show one target company plus mission, run, and related dossiers.
    [HttpGet("{id:guid}")]
    public IActionResult Details(Guid id)
    {
        var company = _repository.GetTargetCompany(id);
        if (company is null)
        {
            return NotFound();
        }

        // Resolve the owning run, mission, and company-linked dossiers.
        var run = _repository.GetMissionRuns().FirstOrDefault(item => item.TargetCompanies.Any(candidate => candidate.Id == id));
        var mission = _repository.GetMissions().FirstOrDefault(item => item.Runs.Any(candidate => candidate.TargetCompanies.Any(companyCandidate => companyCandidate.Id == id)));
        var dossiers = run?.LeadDossiers.Where(dossier => dossier.TargetCompanyId == id).ToList() ?? [];

        // Send the assembled view model to the details page.
        return View(new TargetCompanyDetailsViewModel
        {
            Company = company,
            Mission = mission,
            Run = run,
            Dossiers = dossiers
        });
    }

    [HttpGet("new")]
    public IActionResult Create()
    {
        return View(new TargetCompanyFormViewModel
        {
            HeadquartersCountry = "United States",
            MatchScore = 0.50m
        });
    }

    [HttpPost("new")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TargetCompanyFormViewModel model)
    {
        await ValidateCompany(model);
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var entity = new TargetCompany
        {
            Id = Guid.NewGuid(),
            MissionRunId = model.MissionRunId,
            Name = model.Name.Trim(),
            Domain = model.Domain.Trim(),
            Industry = model.Industry.Trim(),
            HeadquartersCity = model.HeadquartersCity.Trim(),
            HeadquartersCountry = model.HeadquartersCountry.Trim(),
            OrganizationStageLabel = model.OrganizationStageLabel?.Trim(),
            LastSignalAtUtc = model.LastSignalAtUtc.HasValue ? NormalizeUtc(model.LastSignalAtUtc.Value) : null,
            EmployeeCount = model.EmployeeCount,
            IsHeadquartersVerified = model.IsHeadquartersVerified,
            MatchScore = model.MatchScore
        };

        _dbContext.TargetCompanies.Add(entity);
        await _dbContext.SaveChangesAsync();

        return RedirectToAction(nameof(Details), new { id = entity.Id });
    }

    [HttpGet("{id:guid}/edit")]
    public async Task<IActionResult> Edit(Guid id)
    {
        var company = await _dbContext.TargetCompanies.AsNoTracking()
            .Include(item => item.MissionRun)
            .FirstOrDefaultAsync(item => item.Id == id);
        if (company is null)
        {
            return NotFound();
        }

        return View(ToForm(company));
    }

    [HttpPost("{id:guid}/edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, TargetCompanyFormViewModel model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        var company = await _dbContext.TargetCompanies.FirstOrDefaultAsync(item => item.Id == id);
        if (company is null)
        {
            return NotFound();
        }

        await ValidateCompany(model);
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        company.MissionRunId = model.MissionRunId;
        company.Name = model.Name.Trim();
        company.Domain = model.Domain.Trim();
        company.Industry = model.Industry.Trim();
        company.HeadquartersCity = model.HeadquartersCity.Trim();
        company.HeadquartersCountry = model.HeadquartersCountry.Trim();
        company.OrganizationStageLabel = model.OrganizationStageLabel?.Trim();
        company.LastSignalAtUtc = model.LastSignalAtUtc.HasValue ? NormalizeUtc(model.LastSignalAtUtc.Value) : null;
        company.EmployeeCount = model.EmployeeCount;
        company.IsHeadquartersVerified = model.IsHeadquartersVerified;
        company.MatchScore = model.MatchScore;

        await _dbContext.SaveChangesAsync();

        return RedirectToAction(nameof(Details), new { id = company.Id });
    }

    [HttpGet("{id:guid}/delete")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var company = await _dbContext.TargetCompanies.AsNoTracking()
            .Include(item => item.Contacts)
            .Include(item => item.LeadDossiers)
            .FirstOrDefaultAsync(item => item.Id == id);
        if (company is null)
        {
            return NotFound();
        }

        return View("~/Views/Shared/DeleteEntity.cshtml", new DeleteEntityViewModel
        {
            Id = company.Id,
            EntityName = "Companies",
            Title = company.Name,
            Description = "Deleting a company removes its contacts, channels, evidence, and linked dossiers.",
            ReturnController = "TargetCompanies",
            Warnings =
            [
                $"{company.Contacts.Count} contact(s) will be removed.",
                $"{company.LeadDossiers.Count} dossier(s) will be removed."
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

        var company = await _dbContext.TargetCompanies.FirstOrDefaultAsync(item => item.Id == id);
        if (company is null)
        {
            return NotFound();
        }

        var dossiers = await _dbContext.LeadDossiers.Where(dossier => dossier.TargetCompanyId == id).ToListAsync();
        _dbContext.LeadDossiers.RemoveRange(dossiers);
        _dbContext.TargetCompanies.Remove(company);
        await _dbContext.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    private async Task ValidateCompany(TargetCompanyFormViewModel model)
    {
        if (model.MissionRunId == Guid.Empty || !await _dbContext.MissionRuns.AnyAsync(run => run.Id == model.MissionRunId))
        {
            ModelState.AddModelError(nameof(model.MissionRunId), "Select an existing run.");
        }
    }

    private static TargetCompanyFormViewModel ToForm(TargetCompany company)
    {
        return new TargetCompanyFormViewModel
        {
            Id = company.Id,
            MissionRunId = company.MissionRunId,
            MissionRunName = company.MissionRun?.RunCode,
            Name = company.Name,
            Domain = company.Domain,
            Industry = company.Industry,
            HeadquartersCity = company.HeadquartersCity,
            HeadquartersCountry = company.HeadquartersCountry,
            OrganizationStageLabel = company.OrganizationStageLabel,
            LastSignalAtUtc = company.LastSignalAtUtc,
            EmployeeCount = company.EmployeeCount,
            IsHeadquartersVerified = company.IsHeadquartersVerified,
            MatchScore = company.MatchScore
        };
    }

    private static DateTime NormalizeUtc(DateTime value)
    {
        return value.Kind == DateTimeKind.Utc ? value : DateTime.SpecifyKind(value, DateTimeKind.Utc);
    }
}
