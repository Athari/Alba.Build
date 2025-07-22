using System.Security;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Alba.Build.PowerShell.UI.Wpf.Prompts;

internal partial class SecureReadLineModel : PromptModelBase<SecureString?>
{
    [ObservableProperty]
    public partial SecureString SecureLine { get; set; } = new();

    public override void OnOk() => Success(SecureLine);
}