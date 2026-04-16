using leadgen.Services;
using leadgen.ViewModels.SwarmAgents;
using Microsoft.AspNetCore.Mvc;

namespace leadgen.Controllers;

public sealed class SwarmAgentsController : Controller
{
    private readonly ILeadgenReadRepository _repository;

    public SwarmAgentsController(ILeadgenReadRepository repository)
    {
        _repository = repository;
    }

    public IActionResult Index()
    {
        var agents = _repository.GetSwarmAgents()
            .OrderBy(agent => agent.Role)
            .ThenBy(agent => agent.CodeName)
            .ToList();

        return View(agents);
    }

    public IActionResult Details(Guid id)
    {
        var agent = _repository.GetSwarmAgent(id);
        if (agent is null)
        {
            return NotFound();
        }

        var assignments = _repository.GetMissionAgentAssignments()
            .Where(assignment => assignment.SwarmAgentId == id)
            .OrderByDescending(assignment => assignment.AssignedAtUtc)
            .ToList();

        return View(new SwarmAgentDetailsViewModel
        {
            Agent = agent,
            Assignments = assignments
        });
    }
}
