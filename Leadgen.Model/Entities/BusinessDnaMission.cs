using Leadgen.Model.Enums;

namespace Leadgen.Model.Entities;

public class BusinessDnaMission
{
    public Guid Id { get; set; }

    public string MissionName { get; set; } = string.Empty;

    public string ProductName { get; set; } = string.Empty;

    public string Mechanic { get; set; } = string.Empty;

    public string PrimarySurface { get; set; } = string.Empty;

    public List<string> SurfaceTags { get; set; } = new();

    public string Persona { get; set; } = string.Empty;

    public string Villain { get; set; } = string.Empty;

    public string Delta { get; set; } = string.Empty;

    public decimal ConfidenceScore { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public MissionStatus Status { get; set; }

    public List<ClarificationQuestion> ClarificationQuestions { get; set; } = new();

    public List<MissionRun> Runs { get; set; } = new();
}
