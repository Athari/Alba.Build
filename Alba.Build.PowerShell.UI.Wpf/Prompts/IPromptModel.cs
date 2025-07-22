using Alba.Build.PowerShell.Common;

namespace Alba.Build.PowerShell.UI.Wpf.Prompts;

internal interface IPromptModel<T>
{
    Task<Result<T>> GetResultAsync();
}