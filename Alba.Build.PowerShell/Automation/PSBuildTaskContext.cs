using Alba.Build.PowerShell.Tasks;

namespace Alba.Build.PowerShell;

internal class PSBuildTaskContext(PSShell shell, PSBuildHost host, ExecPowerShellTask task)
    : PSBaseTaskContext(shell, host, task)
{
    public new PSBuildHost Host {
        get => (PSBuildHost)base.Host;
        private protected set => base.Host = value;
    }

    public void Deconstruct(out PSShell shell, out PSBuildHost host, out ExecPowerShellTask task) =>
        (task, shell, host) = (Task, Shell, Host);
}