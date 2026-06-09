using Leadgen.Model.Entities;
using leadgen.Data;
using leadgen.Services;
using leadgen.ViewModels.Crud;
using leadgen.ViewModels.SwarmAgents;
using leadgen.ViewModels.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace leadgen.Controllers;

// Expose swarm agent list and details pages.
[Authorize]
[Route("agents")]
public sealed class SwarmAgentsController : Controller
{
    // Read-only access to the seeded Leadgen dataset.
    private readonly ILeadgenReadRepository _repository;
    private readonly LeadgenDbContext _dbContext;

    // Receive the repository from dependency injection.
    public SwarmAgentsController(ILeadgenReadRepository repository, LeadgenDbContext dbContext)
    {
        _repository = repository;
        _dbContext = dbContext;
    }

    // Show all agents ordered by role and code name.
    [AllowAnonymous]
    [HttpGet("")]
    public IActionResult Index()
    {
        var agents = _repository.GetSwarmAgents()
            .OrderBy(agent => agent.Role)
            .ThenBy(agent => agent.CodeName)
            .ToList();

        return View(agents);
    }

    // Show one agent and all assignments that reference it.
    [HttpGet("{id:guid}")]
    public IActionResult Details(Guid id)
    {
        var agent = _repository.GetSwarmAgent(id);
        if (agent is null)
        {
            return NotFound();
        }

        // Resolve the assignments performed by this agent.
        var assignments = _repository.GetMissionAgentAssignments()
            .Where(assignment => assignment.SwarmAgentId == id)
            .OrderByDescending(assignment => assignment.AssignedAtUtc)
            .ToList();

        // Build the view model used by the agent details page.
        return View(new SwarmAgentDetailsViewModel
        {
            Agent = agent,
            Assignments = assignments
        });
    }

    [Authorize(Roles = "Admin,Manager")]
    [HttpGet("new")]
    public IActionResult Create()
    {
        return View(new SwarmAgentFormViewModel
        {
            Provider = "OpenAI",
            IsActive = true,
            LastHeartbeatUtc = DateTime.UtcNow,
            MaxConcurrentTasks = 1
        });
    }

    [Authorize(Roles = "Admin,Manager")]
    [HttpPost("new")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(SwarmAgentFormViewModel model)
    {
        await ValidateAgent(model);
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var entity = new SwarmAgent
        {
            Id = Guid.NewGuid(),
            CodeName = model.CodeName.Trim(),
            Role = model.Role,
            Provider = model.Provider.Trim(),
            Temperature = model.Temperature,
            MaxConcurrentTasks = model.MaxConcurrentTasks,
            IsActive = model.IsActive,
            LastHeartbeatUtc = NormalizeUtc(model.LastHeartbeatUtc),
            CurrentFocus = model.CurrentFocus.Trim()
        };

        _dbContext.SwarmAgents.Add(entity);
        await _dbContext.SaveChangesAsync();

        return RedirectToAction(nameof(Details), new { id = entity.Id });
    }

    [Authorize(Roles = "Admin,Manager")]
    [HttpGet("{id:guid}/edit")]
    public async Task<IActionResult> Edit(Guid id)
    {
        var agent = await _dbContext.SwarmAgents.AsNoTracking().FirstOrDefaultAsync(item => item.Id == id);
        if (agent is null)
        {
            return NotFound();
        }

        return View(ToForm(agent));
    }

    [Authorize(Roles = "Admin,Manager")]
    [HttpPost("{id:guid}/edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, SwarmAgentFormViewModel model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        var agent = await _dbContext.SwarmAgents.FirstOrDefaultAsync(item => item.Id == id);
        if (agent is null)
        {
            return NotFound();
        }

        await ValidateAgent(model, id);
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        agent.CodeName = model.CodeName.Trim();
        agent.Role = model.Role;
        agent.Provider = model.Provider.Trim();
        agent.Temperature = model.Temperature;
        agent.MaxConcurrentTasks = model.MaxConcurrentTasks;
        agent.IsActive = model.IsActive;
        agent.LastHeartbeatUtc = NormalizeUtc(model.LastHeartbeatUtc);
        agent.CurrentFocus = model.CurrentFocus.Trim();

        await _dbContext.SaveChangesAsync();

        return RedirectToAction(nameof(Details), new { id = agent.Id });
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("{id:guid}/delete")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var agent = await _dbContext.SwarmAgents.AsNoTracking()
            .Include(item => item.MissionAssignments)
            .FirstOrDefaultAsync(item => item.Id == id);
        if (agent is null)
        {
            return NotFound();
        }

        return View("~/Views/Shared/DeleteEntity.cshtml", new DeleteEntityViewModel
        {
            Id = agent.Id,
            EntityName = "Agents",
            Title = agent.CodeName,
            Description = "Deleting an agent removes its historical assignment links first so the agent record can be deleted cleanly.",
            ReturnController = "SwarmAgents",
            Warnings = [$"{agent.MissionAssignments.Count} assignment link(s) will be removed."]
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

        var agent = await _dbContext.SwarmAgents.FirstOrDefaultAsync(item => item.Id == id);
        if (agent is null)
        {
            return NotFound();
        }

        var assignments = await _dbContext.MissionAgentAssignments.Where(assignment => assignment.SwarmAgentId == id).ToListAsync();
        _dbContext.MissionAgentAssignments.RemoveRange(assignments);
        _dbContext.SwarmAgents.Remove(agent);
        await _dbContext.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    private async Task ValidateAgent(SwarmAgentFormViewModel model, Guid? currentId = null)
    {
        if (await _dbContext.SwarmAgents.AnyAsync(agent => agent.CodeName == model.CodeName.Trim() && agent.Id != currentId))
        {
            ModelState.AddModelError(nameof(model.CodeName), "Code name must be unique.");
        }
    }

    private static SwarmAgentFormViewModel ToForm(SwarmAgent agent)
    {
        return new SwarmAgentFormViewModel
        {
            Id = agent.Id,
            CodeName = agent.CodeName,
            Role = agent.Role,
            Provider = agent.Provider,
            Temperature = agent.Temperature,
            MaxConcurrentTasks = agent.MaxConcurrentTasks,
            IsActive = agent.IsActive,
            LastHeartbeatUtc = agent.LastHeartbeatUtc,
            CurrentFocus = agent.CurrentFocus
        };
    }

    private static DateTime NormalizeUtc(DateTime value)
    {
        return value.Kind == DateTimeKind.Utc ? value : DateTime.SpecifyKind(value, DateTimeKind.Utc);
    }
}
