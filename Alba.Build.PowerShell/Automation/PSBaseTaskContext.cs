using Alba.Build.PowerShell.Tasks;

namespace Alba.Build.PowerShell;

internal class PSBaseTaskContext(PSShell shell, PSBaseHost host, ExecPowerShellTask task)
{
    public PSShell Shell { get; } = shell;
    public PSBaseHost Host { get; private protected set; } = host;
    public ExecPowerShellTask Task { get; private protected set; } = task;

    public void Deconstruct(out PSShell shell, out PSBaseHost host, out ExecPowerShellTask task) =>
        (task, shell, host) = (Task, Shell, Host);
}