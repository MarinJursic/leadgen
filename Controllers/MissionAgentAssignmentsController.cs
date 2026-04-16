using leadgen.Services;
using leadgen.ViewModels.MissionAgentAssignments;
using Microsoft.AspNetCore.Mvc;

namespace leadgen.Controllers;

public sealed class MissionAgentAssignmentsController : Controller
{
    private readonly ILeadgenReadRepository _repository;

    public MissionAgentAssignmentsController(ILeadgenReadRepository repository)
    {
        _repository = repository;
    }

    public IActionResult Index()
    {
        var assignments = _repository.GetMissionAgentAssignments()
            .OrderByDescending(assignment => assignment.AssignedAtUtc)
            .ToList();

        return View(assignments);
    }

    public IActionResult Details(Guid id)
    {
        var assignment = _repository.GetMissionAgentAssignment(id);
        if (assignment is null)
        {
            return NotFound();
        }

        var run = _repository.GetMissionRuns().FirstOrDefault(item => item.Id == assignment.MissionRunId);
        var mission = _repository.GetMissions().FirstOrDefault(item => item.Runs.Any(candidate => candidate.Id == assignment.MissionRunId));
        var agent = _repository.GetSwarmAgent(assignment.SwarmAgentId);

        return View(new MissionAgentAssignmentDetailsViewModel
        {
            Assignment = assignment,
            Run = run,
            Mission = mission,
            Agent = agent
        });
    }
}
