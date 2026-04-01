using Leadgen.Model.Enums;

namespace Leadgen.Model.Entities;

public class MissionRun
{
    public Guid Id { get; set; }

    public string RunCode { get; set; } = string.Empty;

    public Guid BusinessDnaMissionId { get; set; }

    public DateTime StartedAtUtc { get; set; }

    public DateTime? CompletedAtUtc { get; set; }

    public MissionStatus Status { get; set; }

    public string SearchRegion { get; set; } = string.Empty;

    public int TokenBudget { get; set; }

    public decimal EstimatedCostUsd { get; set; }

    public List<MissionAgentAssignment> AgentAssignments { get; set; } = new();

    public List<TargetCompany> TargetCompanies { get; set; } = new();

    public List<LeadDossier> LeadDossiers { get; set; } = new();
}
