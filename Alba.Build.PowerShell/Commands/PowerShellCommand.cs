using System.Management.Automation;

namespace Alba.Build.PowerShell.Commands;

public abstract class PowerShellCommand : PSCmdlet
{
    private protected PowerShellTaskContext Context
    {
        get
        {
            if (Host.PrivateData?.BaseObject is not PowerShellTaskContext ctx)
                throw new InvalidOperationException($"Host must be ${typeof(PowerShellHost).FullName}.");
            return ctx;
        }
    }
}