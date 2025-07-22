using Alba.Build.PowerShell.Common;

namespace Alba.Build.PowerShell.UI.Wpf.Prompts;

internal abstract class PromptModelBase<T> : PromptModelBase, IPromptModel<T>
{
    private readonly TaskCompletionSource<Result<T>> _tcs = new();

    public void Success(T value) => _tcs.SetResult(Result.Success(value));
    public void Cancel() => _tcs.SetResult(Result.Cancel<T>());
    public void Error(Exception e) => _tcs.SetResult(Result.Error<T>(e));

    public Task<Result<T>> GetResultAsync() => _tcs.Task;

    public override void OnCancel() => Cancel();
}