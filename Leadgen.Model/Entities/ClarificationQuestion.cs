namespace Leadgen.Model.Entities;

public class ClarificationQuestion
{
    public Guid Id { get; set; }

    public string SlotName { get; set; } = string.Empty;

    public string Prompt { get; set; } = string.Empty;

    public string Reason { get; set; } = string.Empty;

    public bool IsAnswered { get; set; }

    public string? Answer { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? AnsweredAtUtc { get; set; }
}
