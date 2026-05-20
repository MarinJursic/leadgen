namespace leadgen.ViewModels.Shared;

public sealed class AutocompleteFieldViewModel
{
    public string Name { get; init; } = string.Empty;

    public string Label { get; init; } = string.Empty;

    public string Endpoint { get; init; } = string.Empty;

    public Guid? Value { get; init; }

    public string? DisplayValue { get; init; }

    public string Placeholder { get; init; } = "Search...";

    public bool IsRequired { get; init; } = true;
}
