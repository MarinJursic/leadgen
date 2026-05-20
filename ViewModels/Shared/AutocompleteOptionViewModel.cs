namespace leadgen.ViewModels.Shared;

public sealed class AutocompleteOptionViewModel
{
    public Guid Id { get; init; }

    public string Text { get; init; } = string.Empty;

    public string? Description { get; init; }
}
