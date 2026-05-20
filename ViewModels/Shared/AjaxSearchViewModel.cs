namespace leadgen.ViewModels.Shared;

public sealed class AjaxSearchViewModel
{
    public AjaxSearchViewModel(string entity, string placeholder)
    {
        Entity = entity;
        Placeholder = placeholder;
    }

    public string Entity { get; }

    public string Placeholder { get; }
}
