using System.Diagnostics.CodeAnalysis;
using System.Windows.Input;
using Alba.Build.PowerShell.UI.Wpf.Common;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Alba.Build.PowerShell.UI.Wpf.Main;

internal partial class MainModel : ModelBase
{
    [ObservableProperty]
    public partial string Title { get; set; } = "MSBuild PowerShell UI";

    [ObservableProperty]
    public partial PromptModelBase? Prompt { get; set; }

    public Task<Result<string?>> ReadLine(string? prompt)
    {
        var m = new ReadLineModel {
            Header = prompt ?? "Please enter text",
        };
        Prompt = m;
        return m.GetResultAsync();
    }
}

internal abstract partial class PromptModelBase : ModelBase
{
    [ObservableProperty]
    public partial string Header { get; set; } = "Header";

    [field: MaybeNull]
    public ICommand OkCommand => field ??= new RelayCommand(OnOk);

    [field: MaybeNull]
    public ICommand CancelCommand => field ??= new RelayCommand(OnCancel);

    public abstract void OnOk();
    public abstract void OnCancel();
}

internal abstract class PromptModelBase<T> : PromptModelBase
{
    private readonly TaskCompletionSource<Result<T>> _tcs = new();

    public void Success(T value) => _tcs.SetResult(Result.Success(value));
    public void Cancel() => _tcs.SetResult(Result.Cancel<T>());
    public void Error(Exception e) => _tcs.SetResult(Result.Error<T>(e));

    public Task<Result<T>> GetResultAsync() => _tcs.Task;
}

internal partial class ReadLineModel : PromptModelBase<string?>
{
    [ObservableProperty]
    public partial string Line { get; set; } = "";

    public override void OnOk() => Success(Line);

    public override void OnCancel() => Cancel();
}