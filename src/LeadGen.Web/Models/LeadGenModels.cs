using System.ComponentModel.DataAnnotations;
using LeadGen.Core.Domain;
using LeadGen.Core.Services;

namespace LeadGen.Web.Models;

public sealed class CampaignFormModel
{
    public Guid? Id { get; set; }

    [StringLength(160)]
    [Display(Name = "Campaign name")]
    public string? Name { get; set; }

    [Required, StringLength(160)]
    [Display(Name = "Business name")]
    public string BusinessName { get; set; } = string.Empty;

    [StringLength(500)]
    [Display(Name = "Website URL")]
    public string? WebsiteUrl { get; set; }

    [Required, StringLength(4000)]
    [Display(Name = "What the business does")]
    public string BusinessDescription { get; set; } = string.Empty;

    [Required, StringLength(500)]
    [Display(Name = "Business location")]
    public string? TargetGeography { get; set; }

    [StringLength(1000)]
    [Display(Name = "Target customers")]
    public string? TargetCustomers { get; set; }

    [StringLength(1000)]
    public string? Exclusions { get; set; }

    [Display(Name = "ICP JSON")]
    public string? IcpJson { get; set; }

    [Range(1, 25)]
    [Display(Name = "Lead count")]
    public int RequestedLeadCount { get; set; } = 5;
}

public sealed class LeadFormModel
{
    public Guid? Id { get; set; }

    [Required]
    public Guid CampaignId { get; set; }

    [Required, StringLength(200)]
    [Display(Name = "Company name")]
    public string CompanyName { get; set; } = string.Empty;

    [StringLength(200)]
    public string? Domain { get; set; }

    [StringLength(500)]
    [Display(Name = "Website URL")]
    public string? WebsiteUrl { get; set; }

    [StringLength(160)]
    public string? Industry { get; set; }

    [StringLength(160)]
    public string? Location { get; set; }

    [Range(0, 100)]
    [Display(Name = "Fit score")]
    public int FitScore { get; set; } = 70;

    [Range(0, 100)]
    [Display(Name = "Confidence score")]
    public int ConfidenceScore { get; set; } = 65;

    public LeadStatus Status { get; set; } = LeadStatus.New;

    [Display(Name = "Match reasons JSON")]
    public string MatchReasonsJson { get; set; } = "[]";

    [Display(Name = "Evidence JSON")]
    public string EvidenceJson { get; set; } = "[]";

    [Required, Display(Name = "Dossier markdown")]
    public string DossierMarkdown { get; set; } = string.Empty;

    [StringLength(1000)]
    [Display(Name = "Suggested outreach angle")]
    public string? SuggestedOutreachAngle { get; set; }
}

public sealed class LeadContactFormModel
{
    public Guid? Id { get; set; }

    [Required]
    public Guid LeadId { get; set; }

    public LeadContactType Type { get; set; } = LeadContactType.ContactPage;

    [Required, StringLength(500)]
    public string Value { get; set; } = string.Empty;

    [StringLength(500)]
    [Display(Name = "Source URL")]
    public string? SourceUrl { get; set; }

    [Range(0, 100)]
    [Display(Name = "Confidence score")]
    public int ConfidenceScore { get; set; } = 60;

    [Display(Name = "Verified")]
    public bool IsVerified { get; set; }
}

public sealed class LeadNoteFormModel
{
    public Guid? Id { get; set; }

    [Required]
    public Guid LeadId { get; set; }

    [Required, StringLength(4000)]
    public string Body { get; set; } = string.Empty;
}

public sealed record DashboardViewModel(
    IReadOnlyList<Campaign> RecentCampaigns,
    IReadOnlyList<LeadSearchRun> RecentRuns,
    IReadOnlyList<Lead> RecentLeads);

public sealed record LeadDetailsViewModel(
    Lead Lead,
    LeadContactFormModel ContactForm,
    LeadNoteFormModel NoteForm);

public sealed record LeadIndexViewModel(
    Guid? SelectedCampaignId,
    int TotalLeadCount,
    IReadOnlyList<Campaign> Campaigns,
    IReadOnlyList<LeadCampaignGroupViewModel> CampaignGroups);

public sealed record LeadCampaignGroupViewModel(
    Campaign Campaign,
    IReadOnlyList<Lead> Leads);

public sealed record SearchPageViewModel(
    string? Query,
    IReadOnlyList<GlobalSearchResult> Results);

public sealed record ApiErrorEnvelope(ApiError Error);

public sealed record ApiError(string Code, string Message, string CorrelationId);

public sealed record CampaignDto(
    Guid Id,
    string Name,
    string BusinessName,
    string? WebsiteUrl,
    string BusinessDescription,
    string? TargetGeography,
    string? TargetCustomers,
    string? Exclusions,
    string? IcpJson,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc,
    int RunCount,
    int LeadCount);

public sealed class CampaignWriteRequest
{
    [StringLength(160)]
    public string? Name { get; set; }

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
}

public sealed class StartRunRequest
{
    [Range(1, 25)]
    public int RequestedLeadCount { get; set; } = 5;
}

public sealed record RunDto(
    Guid Id,
    Guid CampaignId,
    LeadSearchRunStatus Status,
    int RequestedLeadCount,
    string? SearchQueriesJson,
    DateTime? StartedAtUtc,
    DateTime? CompletedAtUtc,
    string? ErrorMessage,
    decimal EstimatedCostUsd,
    string? LogsJson,
    int LeadCount);

public sealed record LeadDto(
    Guid Id,
    Guid CampaignId,
    Guid? LeadSearchRunId,
    string CompanyName,
    string? Domain,
    string? WebsiteUrl,
    string? Industry,
    string? Location,
    int FitScore,
    int ConfidenceScore,
    LeadStatus Status,
    string MatchReasonsJson,
    string EvidenceJson,
    string DossierMarkdown,
    string? SuggestedOutreachAngle,
    IReadOnlyList<LeadContactDto> Contacts,
    IReadOnlyList<LeadNoteDto> Notes);

public sealed class LeadWriteRequest
{
    [Required]
    public Guid CampaignId { get; set; }

    [Required, StringLength(200)]
    public string CompanyName { get; set; } = string.Empty;

    [StringLength(200)]
    public string? Domain { get; set; }

    [StringLength(500)]
    public string? WebsiteUrl { get; set; }

    [StringLength(160)]
    public string? Industry { get; set; }

    [StringLength(160)]
    public string? Location { get; set; }

    [Range(0, 100)]
    public int FitScore { get; set; } = 70;

    [Range(0, 100)]
    public int ConfidenceScore { get; set; } = 65;

    public LeadStatus Status { get; set; } = LeadStatus.New;

    public string MatchReasonsJson { get; set; } = "[]";

    public string EvidenceJson { get; set; } = "[]";

    [Required]
    public string DossierMarkdown { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? SuggestedOutreachAngle { get; set; }
}

public sealed record LeadContactDto(
    Guid Id,
    Guid LeadId,
    LeadContactType Type,
    string Value,
    string? SourceUrl,
    int ConfidenceScore,
    bool IsVerified);

public sealed class LeadContactWriteRequest
{
    public LeadContactType Type { get; set; } = LeadContactType.ContactPage;

    [Required, StringLength(500)]
    public string Value { get; set; } = string.Empty;

    [StringLength(500)]
    public string? SourceUrl { get; set; }

    [Range(0, 100)]
    public int ConfidenceScore { get; set; } = 60;

    public bool IsVerified { get; set; }
}

public sealed record LeadNoteDto(Guid Id, Guid LeadId, string Body, DateTime CreatedAtUtc, DateTime UpdatedAtUtc);

public sealed class LeadNoteWriteRequest
{
    [Required, StringLength(4000)]
    public string Body { get; set; } = string.Empty;
}

public sealed record SearchResultDto(string Type, string Title, string Subtitle, string Url);
