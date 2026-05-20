namespace leadgen.ViewModels.Shared;

public sealed class TableSearchRowViewModel
{
    public IReadOnlyList<TableSearchCellViewModel> Cells { get; init; } = Array.Empty<TableSearchCellViewModel>();

    public IReadOnlyList<TableSearchActionViewModel> Actions { get; init; } = Array.Empty<TableSearchActionViewModel>();
}
