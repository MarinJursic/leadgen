using System.ComponentModel.DataAnnotations;

namespace LeadGen.Core.Domain;

public enum LeadSearchRunStatus
{
    Queued,
    Running,
    Completed,
    Failed,
    Cancelled
}

public enum LeadStatus
{
    New,
    Reviewed,
    Contacted,
    Qualified,
    Rejected
}

public enum LeadContactType
{
    Email,
    ContactPage,
    Phone,
    Social,
    Other
}

public sealed class Campaign
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, StringLength(160)]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(160)]
    public string BusinessName { get; set; } = string.Empty;

    [StringLength(500)]
    public string? WebsiteUrl { get; set; }

    [Required, StringLength(4000)]
    public string BusinessDescription { get; set; } = string.Empty;

    [StringLength(500)]
    public string? TargetGeography { get; set; }

    [StringLength(1000)]
    public string? TargetCustomers { get; set; }

    [StringLength(1000)]
    public string? Exclusions { get; set; }

    public string? IcpJson { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    public List<LeadSearchRun> Runs { get; set; } = [];

    public List<Lead> Leads { get; set; } = [];
}

public sealed class LeadSearchRun
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid CampaignId { get; set; }

    public Campaign? Campaign { get; set; }

    public LeadSearchRunStatus Status { get; set; } = LeadSearchRunStatus.Queued;

    public int RequestedLeadCount { get; set; } = 5;

    public string? SearchQueriesJson { get; set; }

    public DateTime? StartedAtUtc { get; set; }

    public DateTime? CompletedAtUtc { get; set; }

    [StringLength(1000)]
    public string? ErrorMessage { get; set; }

    public decimal EstimatedCostUsd { get; set; }

    public string? LogsJson { get; set; }

    public List<Lead> Leads { get; set; } = [];
}

public sealed class Lead
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid CampaignId { get; set; }

    public Campaign? Campaign { get; set; }

    public Guid? LeadSearchRunId { get; set; }

    public LeadSearchRun? LeadSearchRun { get; set; }

    [Required, StringLength(200)]
    public string CompanyName { get; set; } = string.Empty;

    [StringLength(200)]
    public string? Domain { get; set; }

    [Required, StringLength(260)]
    public string DedupeKey { get; set; } = string.Empty;

    [StringLength(500)]
    public string? WebsiteUrl { get; set; }

    [StringLength(160)]
    public string? Industry { get; set; }

    [StringLength(160)]
    public string? Location { get; set; }

    public int FitScore { get; set; }

    public int ConfidenceScore { get; set; }

    public LeadStatus Status { get; set; } = LeadStatus.New;

    public string MatchReasonsJson { get; set; } = "[]";

    public string EvidenceJson { get; set; } = "[]";

    [Required]
    public string DossierMarkdown { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? SuggestedOutreachAngle { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    public List<LeadContact> Contacts { get; set; } = [];

    public List<LeadNote> Notes { get; set; } = [];
}

public sealed class LeadContact
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid LeadId { get; set; }

    public Lead? Lead { get; set; }

    public LeadContactType Type { get; set; } = LeadContactType.Other;

    [Required, StringLength(500)]
    public string Value { get; set; } = string.Empty;

    [StringLength(500)]
    public string? SourceUrl { get; set; }

    public int ConfidenceScore { get; set; }

    public bool IsVerified { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}

public sealed class LeadNote
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid LeadId { get; set; }

    public Lead? Lead { get; set; }

    [Required, StringLength(4000)]
    public string Body { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}

public sealed class AiCallLog
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, StringLength(80)]
    public string Purpose { get; set; } = string.Empty;

    [Required, StringLength(80)]
    public string Provider { get; set; } = string.Empty;

    [Required, StringLength(120)]
    public string Model { get; set; } = string.Empty;

    public int? InputTokens { get; set; }

    public int? OutputTokens { get; set; }

    public decimal EstimatedCostUsd { get; set; }

    public int DurationMs { get; set; }

    public bool Success { get; set; }

    [StringLength(1000)]
    public string? ErrorMessage { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
