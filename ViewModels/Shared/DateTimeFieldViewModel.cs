namespace leadgen.ViewModels.Shared;

public sealed class DateTimeFieldViewModel
{
    public string Name { get; init; } = string.Empty;

    public string Label { get; init; } = string.Empty;

    public DateTime? Value { get; init; }

    public bool IsRequired { get; init; }

    public bool IncludeTime { get; init; } = true;
}
