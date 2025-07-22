using System.Diagnostics.CodeAnalysis;
using System.Windows.Input;
using Alba.Build.PowerShell.UI.Wpf.Common;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Alba.Build.PowerShell.UI.Wpf.Prompts;

internal abstract partial class PromptModelBase : ModelBase
{
    [ObservableProperty]
    public partial string? Caption { get; set; }

    [ObservableProperty]
    public partial string? Message { get; set; }

    [field: MaybeNull]
    public ICommand OkCommand => field ??= new RelayCommand(OnOk);

    [field: MaybeNull]
    public ICommand CancelCommand => field ??= new RelayCommand(OnCancel);

    public abstract void OnOk();
    public abstract void OnCancel();
}