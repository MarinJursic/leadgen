namespace leadgen.ViewModels.Shared;

public sealed class TableSearchResultViewModel
{
    public int TotalCount { get; init; }

    public IReadOnlyList<TableSearchRowViewModel> Rows { get; init; } = Array.Empty<TableSearchRowViewModel>();

    public IReadOnlyList<OutreachSearchCardViewModel> Cards { get; init; } = Array.Empty<OutreachSearchCardViewModel>();
}
