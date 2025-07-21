using System.Management.Automation;

namespace Alba.Build.PowerShell.Commands;

public abstract class PSBuildCommand : PSCmdlet
{
    private protected PSBuildTaskContext Ctx {
        get {
            if (Host.PrivateData?.BaseObject is not PSBuildTaskContext ctx)
                throw new InvalidOperationException($"Host must be {typeof(PSBuildHost).FullName}.");
            return ctx;
        }
    }
}