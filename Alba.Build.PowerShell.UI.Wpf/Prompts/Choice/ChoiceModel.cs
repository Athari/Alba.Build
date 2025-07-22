using Alba.Build.PowerShell.UI.Wpf.Common;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Alba.Build.PowerShell.UI.Wpf.Prompts.Choice;

internal partial class ChoiceModel : ModelBase
{
    [ObservableProperty]
    public partial bool IsSelected { get; set; }

    public string? Label { get; init; }

    public string? HelpMessage { get; init; }
}