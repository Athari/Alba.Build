using System.Management.Automation;
using System.Security;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Alba.Build.PowerShell.UI.Wpf.Prompts;

internal partial class PromptCredentialModel : PromptModelBase<PSCredential?>
{
    [ObservableProperty]
    public required partial string UserName { get; set; }

    [ObservableProperty]
    public partial SecureString Password { get; set; } = new();

    public string? TargetName { get; init; }

    public PSCredentialTypes AllowedTypes { get; init; }

    public PSCredentialUIOptions Options { get; init; }

    public override void OnOk() => Success(new(UserName, Password));
}