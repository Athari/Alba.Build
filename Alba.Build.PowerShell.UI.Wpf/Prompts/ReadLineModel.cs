using CommunityToolkit.Mvvm.ComponentModel;

namespace Alba.Build.PowerShell.UI.Wpf.Prompts;

internal partial class ReadLineModel : PromptModelBase<string?>
{
    [ObservableProperty]
    public partial string Line { get; set; } = "";

    public override void OnOk() => Success(Line);
}