namespace leadgen.ViewModels.Shared;

public sealed class DeleteEntityViewModel
{
    public Guid Id { get; init; }

    public string EntityName { get; init; } = string.Empty;

    public string Title { get; init; } = string.Empty;

    public string Description { get; init; } = string.Empty;

    public string ReturnController { get; init; } = string.Empty;

    public string ReturnAction { get; init; } = "Index";

    public IReadOnlyList<string> Warnings { get; init; } = Array.Empty<string>();
}
