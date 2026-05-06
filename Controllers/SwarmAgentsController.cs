using leadgen.Services;
using leadgen.ViewModels.SwarmAgents;
using Microsoft.AspNetCore.Mvc;

namespace leadgen.Controllers;

// Expose swarm agent list and details pages.
public sealed class SwarmAgentsController : Controller
{
    // Read-only access to the seeded Leadgen dataset.
    private readonly ILeadgenReadRepository _repository;

    // Receive the repository from dependency injection.
    public SwarmAgentsController(ILeadgenReadRepository repository)
    {
        _repository = repository;
    }

    // Show all agents ordered by role and code name.
    public IActionResult Index()
    {
        var agents = _repository.GetSwarmAgents()
            .OrderBy(agent => agent.Role)
            .ThenBy(agent => agent.CodeName)
            .ToList();

        return View(agents);
    }

    // Show one agent and all assignments that reference it.
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
}
