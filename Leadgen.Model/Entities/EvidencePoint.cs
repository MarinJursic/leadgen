using Leadgen.Model.Enums;

namespace Leadgen.Model.Entities;

public class EvidencePoint
{
    public Guid Id { get; set; }

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
