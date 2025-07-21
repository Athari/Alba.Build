using System.Diagnostics.CodeAnalysis;
using System.Windows.Threading;
using Alba.Build.PowerShell.UI.Wpf.Common;
using Alba.Build.PowerShell.UI.Wpf.Main;
using Alba.Build.PowerShell.UI.Wpf.Tasks;

namespace Alba.Build.PowerShell.UI.Wpf;

[SuppressMessage("Performance", "CA1822:Mark members as static")]
internal class PSWpfBuildTaskContext(PSShell shell, PSWpfBuildHost host, ExecPowerShellWpfTask task)
    : PSBuildTaskContext(shell, host, task)
{
    public Dispatcher Dispatcher => AwaitWpf.UIDispatcher;

    [field: MaybeNull]
    public MainWindow Window => InvokeVerified(() => field ??= new(this));

    [field: MaybeNull]
    public MainModel Model => InvokeVerified(() => field ??= new());

    public new PSWpfBuildHost Host {
        get => (PSWpfBuildHost)base.Host;
        private protected set => base.Host = value;
    }

    public new ExecPowerShellWpfTask Task {
        get => (ExecPowerShellWpfTask)base.Task;
        private protected set => base.Task = value;
    }

    public void Invoke(Action callback,
        DispatcherPriority priority, CancellationToken ct = default) =>
        Dispatcher.Invoke(callback, priority, ct);

    public TResult Invoke<TResult>(Func<TResult> callback,
        DispatcherPriority priority, CancellationToken ct = default) =>
        Dispatcher.Invoke(callback, priority, ct);

    public void Invoke(Action callback, CancellationToken ct = default) =>
        Invoke(callback, DispatcherPriority.Normal, ct);

    public TResult Invoke<TResult>(Func<TResult> callback, CancellationToken ct = default) =>
        Invoke(callback, DispatcherPriority.Normal, ct);

    private static TResult InvokeVerified<TResult>(Func<TResult> callback)
    {
        AwaitWpf.UIDispatcher.VerifyAccess();
        return callback();
    }
}