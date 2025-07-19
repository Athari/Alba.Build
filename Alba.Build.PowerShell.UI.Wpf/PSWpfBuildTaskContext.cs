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
    [field: MaybeNull]
    public MainWindow MainWindow
    {
        get
        {
            //Env.LaunchDebugger();
            AwaitWpf.UIDispatcher.VerifyAccess();
            if (field == null) {
                field = new();
                field.Show();
                field.Activate();
            }
            return field;
        }
    }

    public new PSWpfBuildHost Host
    {
        get => (PSWpfBuildHost)base.Host;
        private protected set => base.Host = value;
    }

    public new ExecPowerShellWpfTask Task
    {
        get => (ExecPowerShellWpfTask)base.Task;
        private protected set => base.Task = value;
    }

    public void Invoke(Action callback,
        DispatcherPriority priority = DispatcherPriority.Normal, CancellationToken ct = default) =>
        AwaitWpf.UIDispatcher.Invoke(callback, priority, ct);

    public TResult Invoke<TResult>(Func<TResult> callback,
        DispatcherPriority priority = DispatcherPriority.Normal, CancellationToken ct = default) =>
        AwaitWpf.UIDispatcher.Invoke(callback, priority, ct);
}