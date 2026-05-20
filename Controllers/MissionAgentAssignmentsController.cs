using Leadgen.Model.Entities;
using leadgen.Data;
using leadgen.Services;
using leadgen.ViewModels.Crud;
using leadgen.ViewModels.MissionAgentAssignments;
using leadgen.ViewModels.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace leadgen.Controllers;

// Expose assignment list and details pages for swarm work allocation.
[Route("assignments")]
public sealed class MissionAgentAssignmentsController : Controller
{
    // Read-only access to the seeded Leadgen dataset.
    private readonly ILeadgenReadRepository _repository;
    private readonly LeadgenDbContext _dbContext;

    // Receive the repository from dependency injection.
    public MissionAgentAssignmentsController(ILeadgenReadRepository repository, LeadgenDbContext dbContext)
    {
        _repository = repository;
        _dbContext = dbContext;
    }

    // Show all assignments ordered by the latest assignment time.
    [HttpGet("")]
    public IActionResult Index()
    {
        var assignments = _repository.GetMissionAgentAssignments()
            .OrderByDescending(assignment => assignment.AssignedAtUtc)
            .ToList();

        return View(assignments);
    }

    // Show one assignment plus the related run, mission, and agent.
    [HttpGet("{id:guid}")]
    public IActionResult Details(Guid id)
    {
        var assignment = _repository.GetMissionAgentAssignment(id);
        if (assignment is null)
        {
            return NotFound();
        }

        // Resolve all related context needed by the details page.
        var run = _repository.GetMissionRuns().FirstOrDefault(item => item.Id == assignment.MissionRunId);
        var mission = _repository.GetMissions().FirstOrDefault(item => item.Runs.Any(candidate => candidate.Id == assignment.MissionRunId));
        var agent = _repository.GetSwarmAgent(assignment.SwarmAgentId);

        // Send the assembled details model to the view.
        return View(new MissionAgentAssignmentDetailsViewModel
        {
            Assignment = assignment,
            Run = run,
            Mission = mission,
            Agent = agent
        });
    }

    [HttpGet("new")]
    public IActionResult Create()
    {
        return View(new MissionAgentAssignmentFormViewModel
        {
            AssignedAtUtc = DateTime.UtcNow,
            Status = Leadgen.Model.Enums.MissionStatus.Running,
            TokenBudget = 1000
        });
    }

    [HttpPost("new")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(MissionAgentAssignmentFormViewModel model)
    {
        await ValidateAssignment(model);
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var entity = new MissionAgentAssignment
        {
            Id = Guid.NewGuid(),
            MissionRunId = model.MissionRunId,
            SwarmAgentId = model.SwarmAgentId,
            AssignedAtUtc = NormalizeUtc(model.AssignedAtUtc),
            Responsibility = model.Responsibility.Trim(),
            TokenBudget = model.TokenBudget,
            Status = model.Status
        };

        _dbContext.MissionAgentAssignments.Add(entity);
        await _dbContext.SaveChangesAsync();

        return RedirectToAction(nameof(Details), new { id = entity.Id });
    }

    [HttpGet("{id:guid}/edit")]
    public async Task<IActionResult> Edit(Guid id)
    {
        var assignment = await _dbContext.MissionAgentAssignments.AsNoTracking()
            .Include(item => item.MissionRun)
            .Include(item => item.SwarmAgent)
            .FirstOrDefaultAsync(item => item.Id == id);
        if (assignment is null)
        {
            return NotFound();
        }

        return View(ToForm(assignment));
    }

    [HttpPost("{id:guid}/edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, MissionAgentAssignmentFormViewModel model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        var assignment = await _dbContext.MissionAgentAssignments.FirstOrDefaultAsync(item => item.Id == id);
        if (assignment is null)
        {
            return NotFound();
        }

        await ValidateAssignment(model);
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        assignment.MissionRunId = model.MissionRunId;
        assignment.SwarmAgentId = model.SwarmAgentId;
        assignment.AssignedAtUtc = NormalizeUtc(model.AssignedAtUtc);
        assignment.Responsibility = model.Responsibility.Trim();
        assignment.TokenBudget = model.TokenBudget;
        assignment.Status = model.Status;

        await _dbContext.SaveChangesAsync();

        return RedirectToAction(nameof(Details), new { id = assignment.Id });
    }

    [HttpGet("{id:guid}/delete")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var assignment = await _dbContext.MissionAgentAssignments.AsNoTracking().FirstOrDefaultAsync(item => item.Id == id);
        if (assignment is null)
        {
            return NotFound();
        }

        return View("~/Views/Shared/DeleteEntity.cshtml", new DeleteEntityViewModel
        {
            Id = assignment.Id,
            EntityName = "Assignments",
            Title = assignment.Responsibility,
            Description = "Deleting an assignment only removes the link between one run and one swarm agent.",
            ReturnController = "MissionAgentAssignments"
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

        var assignment = await _dbContext.MissionAgentAssignments.FirstOrDefaultAsync(item => item.Id == id);
        if (assignment is null)
        {
            return NotFound();
        }

        _dbContext.MissionAgentAssignments.Remove(assignment);
        await _dbContext.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    private async Task ValidateAssignment(MissionAgentAssignmentFormViewModel model)
    {
        if (model.MissionRunId == Guid.Empty || !await _dbContext.MissionRuns.AnyAsync(run => run.Id == model.MissionRunId))
        {
            ModelState.AddModelError(nameof(model.MissionRunId), "Select an existing run.");
        }

        if (model.SwarmAgentId == Guid.Empty || !await _dbContext.SwarmAgents.AnyAsync(agent => agent.Id == model.SwarmAgentId))
        {
            ModelState.AddModelError(nameof(model.SwarmAgentId), "Select an existing agent.");
        }
    }

    private static MissionAgentAssignmentFormViewModel ToForm(MissionAgentAssignment assignment)
    {
        return new MissionAgentAssignmentFormViewModel
        {
            Id = assignment.Id,
            MissionRunId = assignment.MissionRunId,
            MissionRunName = assignment.MissionRun?.RunCode,
            SwarmAgentId = assignment.SwarmAgentId,
            SwarmAgentName = assignment.SwarmAgent?.CodeName,
            AssignedAtUtc = assignment.AssignedAtUtc,
            Responsibility = assignment.Responsibility,
            TokenBudget = assignment.TokenBudget,
            Status = assignment.Status
        };
    }

    private static DateTime NormalizeUtc(DateTime value)
    {
        return value.Kind == DateTimeKind.Utc ? value : DateTime.SpecifyKind(value, DateTimeKind.Utc);
    }
}
