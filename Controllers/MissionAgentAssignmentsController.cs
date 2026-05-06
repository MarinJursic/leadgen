using leadgen.Services;
using leadgen.ViewModels.MissionAgentAssignments;
using Microsoft.AspNetCore.Mvc;

namespace leadgen.Controllers;

// Expose assignment list and details pages for swarm work allocation.
public sealed class MissionAgentAssignmentsController : Controller
{
    // Read-only access to the seeded Leadgen dataset.
    private readonly ILeadgenReadRepository _repository;

    // Receive the repository from dependency injection.
    public MissionAgentAssignmentsController(ILeadgenReadRepository repository)
    {
        _repository = repository;
    }

    // Show all assignments ordered by the latest assignment time.
    public IActionResult Index()
    {
        var assignments = _repository.GetMissionAgentAssignments()
            .OrderByDescending(assignment => assignment.AssignedAtUtc)
            .ToList();

        return View(assignments);
    }

    // Show one assignment plus the related run, mission, and agent.
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
}
