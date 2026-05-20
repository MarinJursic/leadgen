using System.ComponentModel.DataAnnotations;
using Leadgen.Model.Enums;

namespace leadgen.ViewModels.Crud;

public abstract class EntityFormViewModel
{
    public Guid? Id { get; set; }
}

public sealed class MissionFormViewModel : EntityFormViewModel
{
    [Required]
    [StringLength(160)]
    public string MissionName { get; set; } = string.Empty;

    [Required]
    [StringLength(120)]
    public string ProductName { get; set; } = string.Empty;

    [Required]
    [StringLength(500)]
    public string Mechanic { get; set; } = string.Empty;

    [Required]
    [StringLength(120)]
    public string PrimarySurface { get; set; } = string.Empty;

    [Display(Name = "Surface tags")]
    [StringLength(500)]
    public string SurfaceTagsText { get; set; } = string.Empty;

    [Required]
    [StringLength(240)]
    public string Persona { get; set; } = string.Empty;

    [Required]
    [StringLength(500)]
    public string Villain { get; set; } = string.Empty;

    [Required]
    [StringLength(500)]
    public string Delta { get; set; } = string.Empty;

    [Range(0, 1)]
    public decimal ConfidenceScore { get; set; }

    [Required]
    public DateTime CreatedAtUtc { get; set; }

    public MissionStatus Status { get; set; }
}

public sealed class MissionRunFormViewModel : EntityFormViewModel
{
    [Required]
    [StringLength(60)]
    public string RunCode { get; set; } = string.Empty;

    [Display(Name = "Mission")]
    [Required]
    public Guid BusinessDnaMissionId { get; set; }

    public string? BusinessDnaMissionName { get; set; }

    [Required]
    public DateTime StartedAtUtc { get; set; }

    public DateTime? CompletedAtUtc { get; set; }

    public MissionStatus Status { get; set; }

    [Required]
    [StringLength(160)]
    public string SearchRegion { get; set; } = string.Empty;

    [Range(1, 1000000)]
    public int TokenBudget { get; set; }

    [Range(0, 1000000)]
    public decimal EstimatedCostUsd { get; set; }
}

public sealed class MissionAgentAssignmentFormViewModel : EntityFormViewModel
{
    [Display(Name = "Run")]
    [Required]
    public Guid MissionRunId { get; set; }

    public string? MissionRunName { get; set; }

    [Display(Name = "Agent")]
    [Required]
    public Guid SwarmAgentId { get; set; }

    public string? SwarmAgentName { get; set; }

    [Required]
    public DateTime AssignedAtUtc { get; set; }

    [Required]
    [StringLength(500)]
    public string Responsibility { get; set; } = string.Empty;

    [Range(1, 1000000)]
    public int TokenBudget { get; set; }

    public MissionStatus Status { get; set; }
}

public sealed class SwarmAgentFormViewModel : EntityFormViewModel
{
    [Required]
    [StringLength(60)]
    public string CodeName { get; set; } = string.Empty;

    public AgentRole Role { get; set; }

    [Required]
    [StringLength(80)]
    public string Provider { get; set; } = string.Empty;

    [Range(0, 2)]
    public decimal Temperature { get; set; }

    [Range(1, 100)]
    public int MaxConcurrentTasks { get; set; }

    public bool IsActive { get; set; } = true;

    [Required]
    public DateTime LastHeartbeatUtc { get; set; }

    [Required]
    [StringLength(320)]
    public string CurrentFocus { get; set; } = string.Empty;
}

public sealed class TargetCompanyFormViewModel : EntityFormViewModel
{
    [Display(Name = "Run")]
    [Required]
    public Guid MissionRunId { get; set; }

    public string? MissionRunName { get; set; }

    [Required]
    [StringLength(180)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(180)]
    public string Domain { get; set; } = string.Empty;

    [Required]
    [StringLength(160)]
    public string Industry { get; set; } = string.Empty;

    [Required]
    [StringLength(120)]
    public string HeadquartersCity { get; set; } = string.Empty;

    [Required]
    [StringLength(120)]
    public string HeadquartersCountry { get; set; } = string.Empty;

    [StringLength(120)]
    public string? OrganizationStageLabel { get; set; }

    public DateTime? LastSignalAtUtc { get; set; }

    [Range(0, 10000000)]
    public int EmployeeCount { get; set; }

    public bool IsHeadquartersVerified { get; set; }

    [Range(0, 1)]
    public decimal MatchScore { get; set; }
}

public sealed class TargetContactFormViewModel : EntityFormViewModel
{
    [Display(Name = "Company")]
    [Required]
    public Guid TargetCompanyId { get; set; }

    public string? TargetCompanyName { get; set; }

    [Required]
    [StringLength(180)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [StringLength(180)]
    public string JobTitle { get; set; } = string.Empty;

    [Required]
    [StringLength(120)]
    public string Department { get; set; } = string.Empty;

    [Required]
    [StringLength(80)]
    public string Seniority { get; set; } = string.Empty;

    public bool IsDecisionMaker { get; set; }

    [StringLength(320)]
    public string? LinkedInUrl { get; set; }

    [StringLength(120)]
    public string? XHandle { get; set; }

    [StringLength(120)]
    public string? GitHubUsername { get; set; }

    [Required]
    [StringLength(500)]
    public string OpportunitySummary { get; set; } = string.Empty;

    [Required]
    public DateTime LastObservedAtUtc { get; set; }
}

public sealed class ContactChannelFormViewModel : EntityFormViewModel
{
    [Display(Name = "Contact")]
    [Required]
    public Guid TargetContactId { get; set; }

    public string? TargetContactName { get; set; }

    public ContactChannelType Type { get; set; }

    [Required]
    [StringLength(320)]
    public string Value { get; set; } = string.Empty;

    public bool IsVerified { get; set; }

    public DateTime? VerifiedAtUtc { get; set; }

    [Required]
    [StringLength(160)]
    public string Source { get; set; } = string.Empty;

    [Range(0, 1)]
    public decimal ConfidenceScore { get; set; }
}

public sealed class EvidencePointFormViewModel : EntityFormViewModel
{
    [Display(Name = "Contact")]
    [Required]
    public Guid TargetContactId { get; set; }

    public string? TargetContactName { get; set; }

    public EvidenceKind Kind { get; set; }

    [Required]
    [StringLength(160)]
    public string Label { get; set; } = string.Empty;

    [Required]
    [StringLength(160)]
    public string SourcePlatform { get; set; } = string.Empty;

    [Required]
    [StringLength(500)]
    public string SourceUrl { get; set; } = string.Empty;

    [Required]
    [StringLength(500)]
    public string Summary { get; set; } = string.Empty;

    [Required]
    [StringLength(1000)]
    public string RawSnippet { get; set; } = string.Empty;

    [Required]
    public DateTime CapturedAtUtc { get; set; }

    [Range(0, 1)]
    public decimal ConfidenceScore { get; set; }

    public bool IsQualificationSignal { get; set; }
}

public sealed class LeadDossierFormViewModel : EntityFormViewModel
{
    [Display(Name = "Run")]
    [Required]
    public Guid MissionRunId { get; set; }

    public string? MissionRunName { get; set; }

    [Display(Name = "Company")]
    [Required]
    public Guid TargetCompanyId { get; set; }

    public string? TargetCompanyName { get; set; }

    [Display(Name = "Contact")]
    [Required]
    public Guid TargetContactId { get; set; }

    public string? TargetContactName { get; set; }

    [Range(0, 100)]
    public int LeadgenScore { get; set; }

    [Required]
    [StringLength(600)]
    public string SuggestedApproach { get; set; } = string.Empty;

    [Required]
    [StringLength(600)]
    public string AdvantagePoint { get; set; } = string.Empty;

    public bool IsReadyForOutreach { get; set; }

    [Required]
    public DateTime CreatedAtUtc { get; set; }

    [Required]
    public DateTime LastUpdatedAtUtc { get; set; }

    [Range(0, 100000)]
    public int SupportingEvidenceCount { get; set; }
}
