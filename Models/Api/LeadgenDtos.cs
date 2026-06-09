using Leadgen.Model.Enums;
using System.ComponentModel.DataAnnotations;

namespace leadgen.Models.Api;

public sealed class MissionSummaryDto
{
    public Guid Id { get; set; }
    public string MissionName { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public MissionStatus Status { get; set; }
}

public sealed class MissionRunSummaryDto
{
    public Guid Id { get; set; }
    public string RunCode { get; set; } = string.Empty;
    public MissionStatus Status { get; set; }
}

public sealed class SwarmAgentSummaryDto
{
    public Guid Id { get; set; }
    public string CodeName { get; set; } = string.Empty;
    public AgentRole Role { get; set; }
}

public sealed class TargetCompanySummaryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Domain { get; set; } = string.Empty;
}

public sealed class TargetContactSummaryDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string JobTitle { get; set; } = string.Empty;
}

public sealed class BusinessDnaMissionDto
{
    public Guid Id { get; set; }
    public string MissionName { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string Mechanic { get; set; } = string.Empty;
    public string PrimarySurface { get; set; } = string.Empty;
    public IReadOnlyList<string> SurfaceTags { get; set; } = [];
    public string Persona { get; set; } = string.Empty;
    public string Villain { get; set; } = string.Empty;
    public string Delta { get; set; } = string.Empty;
    public decimal ConfidenceScore { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public MissionStatus Status { get; set; }
    public int ClarificationQuestionCount { get; set; }
    public int RunCount { get; set; }
    public int AttachmentCount { get; set; }
}

public sealed class BusinessDnaMissionWriteDto
{
    [Required]
    [MaxLength(160)]
    public string MissionName { get; set; } = string.Empty;

    [Required]
    [MaxLength(120)]
    public string ProductName { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string Mechanic { get; set; } = string.Empty;

    [Required]
    [MaxLength(120)]
    public string PrimarySurface { get; set; } = string.Empty;

    public List<string> SurfaceTags { get; set; } = [];

    [Required]
    [MaxLength(240)]
    public string Persona { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string Villain { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string Delta { get; set; } = string.Empty;

    [Range(0, 1)]
    public decimal ConfidenceScore { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public MissionStatus Status { get; set; }
}

public sealed class ClarificationQuestionDto
{
    public Guid Id { get; set; }
    public MissionSummaryDto? Mission { get; set; }
    public string SlotName { get; set; } = string.Empty;
    public string Prompt { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public bool IsAnswered { get; set; }
    public string? Answer { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? AnsweredAtUtc { get; set; }
}

public sealed class ClarificationQuestionWriteDto
{
    [Required]
    public Guid BusinessDnaMissionId { get; set; }

    [Required]
    [MaxLength(80)]
    public string SlotName { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string Prompt { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string Reason { get; set; } = string.Empty;

    public bool IsAnswered { get; set; }

    [MaxLength(500)]
    public string? Answer { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime? AnsweredAtUtc { get; set; }
}

public sealed class MissionRunDto
{
    public Guid Id { get; set; }
    public string RunCode { get; set; } = string.Empty;
    public MissionSummaryDto? Mission { get; set; }
    public DateTime StartedAtUtc { get; set; }
    public DateTime? CompletedAtUtc { get; set; }
    public MissionStatus Status { get; set; }
    public string SearchRegion { get; set; } = string.Empty;
    public int TokenBudget { get; set; }
    public decimal EstimatedCostUsd { get; set; }
    public int AssignmentCount { get; set; }
    public int CompanyCount { get; set; }
    public int DossierCount { get; set; }
}

public sealed class MissionRunWriteDto
{
    [Required]
    [MaxLength(60)]
    public string RunCode { get; set; } = string.Empty;

    [Required]
    public Guid BusinessDnaMissionId { get; set; }

    public DateTime StartedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAtUtc { get; set; }
    public MissionStatus Status { get; set; }

    [Required]
    [MaxLength(160)]
    public string SearchRegion { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int TokenBudget { get; set; }

    [Range(0, 1000000)]
    public decimal EstimatedCostUsd { get; set; }
}

public sealed class MissionAgentAssignmentDto
{
    public Guid Id { get; set; }
    public MissionRunSummaryDto? MissionRun { get; set; }
    public SwarmAgentSummaryDto? SwarmAgent { get; set; }
    public DateTime AssignedAtUtc { get; set; }
    public string Responsibility { get; set; } = string.Empty;
    public int TokenBudget { get; set; }
    public MissionStatus Status { get; set; }
}

public sealed class MissionAgentAssignmentWriteDto
{
    [Required]
    public Guid MissionRunId { get; set; }

    [Required]
    public Guid SwarmAgentId { get; set; }

    public DateTime AssignedAtUtc { get; set; } = DateTime.UtcNow;

    [Required]
    [MaxLength(500)]
    public string Responsibility { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int TokenBudget { get; set; }

    public MissionStatus Status { get; set; }
}

public sealed class SwarmAgentDto
{
    public Guid Id { get; set; }
    public string CodeName { get; set; } = string.Empty;
    public AgentRole Role { get; set; }
    public string Provider { get; set; } = string.Empty;
    public decimal Temperature { get; set; }
    public int MaxConcurrentTasks { get; set; }
    public bool IsActive { get; set; }
    public DateTime LastHeartbeatUtc { get; set; }
    public string CurrentFocus { get; set; } = string.Empty;
    public int AssignmentCount { get; set; }
}

public sealed class SwarmAgentWriteDto
{
    [Required]
    [MaxLength(60)]
    public string CodeName { get; set; } = string.Empty;

    public AgentRole Role { get; set; }

    [Required]
    [MaxLength(80)]
    public string Provider { get; set; } = string.Empty;

    [Range(0, 2)]
    public decimal Temperature { get; set; }

    [Range(1, 50)]
    public int MaxConcurrentTasks { get; set; }

    public bool IsActive { get; set; }
    public DateTime LastHeartbeatUtc { get; set; } = DateTime.UtcNow;

    [Required]
    [MaxLength(320)]
    public string CurrentFocus { get; set; } = string.Empty;
}

public sealed class TargetCompanyDto
{
    public Guid Id { get; set; }
    public MissionRunSummaryDto? MissionRun { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Domain { get; set; } = string.Empty;
    public string Industry { get; set; } = string.Empty;
    public string HeadquartersCity { get; set; } = string.Empty;
    public string HeadquartersCountry { get; set; } = string.Empty;
    public string? OrganizationStageLabel { get; set; }
    public DateTime? LastSignalAtUtc { get; set; }
    public int EmployeeCount { get; set; }
    public bool IsHeadquartersVerified { get; set; }
    public decimal MatchScore { get; set; }
    public int ContactCount { get; set; }
    public int DossierCount { get; set; }
}

public sealed class TargetCompanyWriteDto
{
    [Required]
    public Guid MissionRunId { get; set; }

    [Required]
    [MaxLength(180)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(180)]
    public string Domain { get; set; } = string.Empty;

    [Required]
    [MaxLength(160)]
    public string Industry { get; set; } = string.Empty;

    [Required]
    [MaxLength(120)]
    public string HeadquartersCity { get; set; } = string.Empty;

    [Required]
    [MaxLength(120)]
    public string HeadquartersCountry { get; set; } = string.Empty;

    [MaxLength(120)]
    public string? OrganizationStageLabel { get; set; }

    public DateTime? LastSignalAtUtc { get; set; }

    [Range(1, int.MaxValue)]
    public int EmployeeCount { get; set; }

    public bool IsHeadquartersVerified { get; set; }

    [Range(0, 1)]
    public decimal MatchScore { get; set; }
}

public sealed class TargetContactDto
{
    public Guid Id { get; set; }
    public TargetCompanySummaryDto? TargetCompany { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string JobTitle { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Seniority { get; set; } = string.Empty;
    public bool IsDecisionMaker { get; set; }
    public string? LinkedInUrl { get; set; }
    public string? XHandle { get; set; }
    public string? GitHubUsername { get; set; }
    public string OpportunitySummary { get; set; } = string.Empty;
    public DateTime LastObservedAtUtc { get; set; }
    public int ContactChannelCount { get; set; }
    public int EvidencePointCount { get; set; }
    public int DossierCount { get; set; }
}

public sealed class TargetContactWriteDto
{
    [Required]
    public Guid TargetCompanyId { get; set; }

    [Required]
    [MaxLength(180)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [MaxLength(180)]
    public string JobTitle { get; set; } = string.Empty;

    [Required]
    [MaxLength(120)]
    public string Department { get; set; } = string.Empty;

    [Required]
    [MaxLength(80)]
    public string Seniority { get; set; } = string.Empty;

    public bool IsDecisionMaker { get; set; }

    [MaxLength(320)]
    public string? LinkedInUrl { get; set; }

    [MaxLength(120)]
    public string? XHandle { get; set; }

    [MaxLength(120)]
    public string? GitHubUsername { get; set; }

    [Required]
    [MaxLength(500)]
    public string OpportunitySummary { get; set; } = string.Empty;

    public DateTime LastObservedAtUtc { get; set; } = DateTime.UtcNow;
}

public sealed class ContactChannelDto
{
    public Guid Id { get; set; }
    public TargetContactSummaryDto? TargetContact { get; set; }
    public ContactChannelType Type { get; set; }
    public string Value { get; set; } = string.Empty;
    public bool IsVerified { get; set; }
    public DateTime? VerifiedAtUtc { get; set; }
    public string Source { get; set; } = string.Empty;
    public decimal ConfidenceScore { get; set; }
}

public sealed class ContactChannelWriteDto
{
    [Required]
    public Guid TargetContactId { get; set; }

    public ContactChannelType Type { get; set; }

    [Required]
    [MaxLength(320)]
    public string Value { get; set; } = string.Empty;

    public bool IsVerified { get; set; }
    public DateTime? VerifiedAtUtc { get; set; }

    [Required]
    [MaxLength(160)]
    public string Source { get; set; } = string.Empty;

    [Range(0, 1)]
    public decimal ConfidenceScore { get; set; }
}

public sealed class EvidencePointDto
{
    public Guid Id { get; set; }
    public TargetContactSummaryDto? TargetContact { get; set; }
    public EvidenceKind Kind { get; set; }
    public string Label { get; set; } = string.Empty;
    public string SourcePlatform { get; set; } = string.Empty;
    public string SourceUrl { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string RawSnippet { get; set; } = string.Empty;
    public DateTime CapturedAtUtc { get; set; }
    public decimal ConfidenceScore { get; set; }
    public bool IsQualificationSignal { get; set; }
}

public sealed class EvidencePointWriteDto
{
    [Required]
    public Guid TargetContactId { get; set; }

    public EvidenceKind Kind { get; set; }

    [Required]
    [MaxLength(160)]
    public string Label { get; set; } = string.Empty;

    [Required]
    [MaxLength(160)]
    public string SourcePlatform { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string SourceUrl { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string Summary { get; set; } = string.Empty;

    [Required]
    [MaxLength(1000)]
    public string RawSnippet { get; set; } = string.Empty;

    public DateTime CapturedAtUtc { get; set; } = DateTime.UtcNow;

    [Range(0, 1)]
    public decimal ConfidenceScore { get; set; }

    public bool IsQualificationSignal { get; set; }
}

public sealed class LeadDossierDto
{
    public Guid Id { get; set; }
    public MissionRunSummaryDto? MissionRun { get; set; }
    public TargetCompanySummaryDto? TargetCompany { get; set; }
    public TargetContactSummaryDto? TargetContact { get; set; }
    public int LeadgenScore { get; set; }
    public string SuggestedApproach { get; set; } = string.Empty;
    public string AdvantagePoint { get; set; } = string.Empty;
    public bool IsReadyForOutreach { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime LastUpdatedAtUtc { get; set; }
    public int SupportingEvidenceCount { get; set; }
}

public sealed class LeadDossierWriteDto
{
    [Required]
    public Guid MissionRunId { get; set; }

    [Required]
    public Guid TargetCompanyId { get; set; }

    [Required]
    public Guid TargetContactId { get; set; }

    [Range(0, 100)]
    public int LeadgenScore { get; set; }

    [Required]
    [MaxLength(600)]
    public string SuggestedApproach { get; set; } = string.Empty;

    [Required]
    [MaxLength(600)]
    public string AdvantagePoint { get; set; } = string.Empty;

    public bool IsReadyForOutreach { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime LastUpdatedAtUtc { get; set; } = DateTime.UtcNow;

    [Range(0, int.MaxValue)]
    public int SupportingEvidenceCount { get; set; }
}

public sealed class MissionAttachmentDto
{
    public Guid Id { get; set; }
    public MissionSummaryDto? Mission { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}

public sealed class MissionAttachmentWriteDto
{
    [Required]
    public Guid BusinessDnaMissionId { get; set; }

    [Required]
    [MaxLength(260)]
    public string FileName { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string FilePath { get; set; } = string.Empty;

    [Required]
    [MaxLength(120)]
    public string ContentType { get; set; } = string.Empty;

    [Range(1, long.MaxValue)]
    public long FileSize { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
