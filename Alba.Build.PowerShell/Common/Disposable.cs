namespace Alba.Build.PowerShell.Common;

public readonly struct Disposable(Action action) : IDisposable
{
    public void Dispose() => action();
}