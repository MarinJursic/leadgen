using Leadgen.Model.Enums;

namespace Leadgen.Model.Entities;

public class ContactChannel
{
    public Guid Id { get; set; }

    public ContactChannelType Type { get; set; }

    public string Value { get; set; } = string.Empty;

    public bool IsVerified { get; set; }

    public DateTime? VerifiedAtUtc { get; set; }

    public string Source { get; set; } = string.Empty;

    public decimal ConfidenceScore { get; set; }
}
