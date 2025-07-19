using Alba.Build.PowerShell.Tasks;
using Alba.Build.PowerShell.UI.Wpf.Common;
using Microsoft.Build.Framework;

namespace Alba.Build.PowerShell.UI.Wpf.Tasks;

[RunInSTA]
public class ExecPowerShellWpfTask : ExecPowerShellTask
{
    public override bool Execute()
    {
        var ret = base.Execute();
        AwaitWpf.StopUIDispatcher();
        return ret;
    }

    private protected override PSBuildHost CreateHost(PSShell ps) => new PSWpfBuildHost(ps, this);
}