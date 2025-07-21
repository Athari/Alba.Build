using Alba.Build.PowerShell.Tasks;
using Alba.Build.PowerShell.UI.Wpf.Common;
using Microsoft.Build.Framework;

namespace Alba.Build.PowerShell.UI.Wpf.Tasks;

[RunInSTA]
public class ExecPowerShellWpfTask : ExecPowerShellTask
{
    internal const string DefaultUITheme = "Luna.NormalColor";
    internal static readonly Version DefaultUIVersion = new(4, 0, 0, 0);

    public double UITimeout { get; set; } = double.PositiveInfinity;

    public string UITheme { get; set; } = DefaultUITheme;

    public Version UIThemeVersion { get; set; } = DefaultUIVersion;

    internal TimeSpan UITimeoutSpan =>
        double.IsInfinity(UITimeout) ? Timeout.InfiniteTimeSpan : TimeSpan.FromSeconds(UITimeout);

    public override bool Execute()
    {
        var ret = base.Execute();
        AwaitWpf.StopUIDispatcher();
        return ret;
    }

    private protected override PSBuildHost CreateHost(PSShell ps) => new PSWpfBuildHost(ps, this);
}