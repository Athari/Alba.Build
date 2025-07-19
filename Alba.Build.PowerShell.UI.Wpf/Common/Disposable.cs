namespace Alba.Build.PowerShell.UI.Wpf.Common;

public readonly struct Disposable(Action action) : IDisposable
{
    public void Dispose() => action();
}