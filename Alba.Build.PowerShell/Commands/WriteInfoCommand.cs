using System.Management.Automation;

namespace Alba.Build.PowerShell.Commands;

[Cmdlet(VerbsCommunications.Write, nameof(LogLevel.Info))]
public class WriteInfoCommand : PowerShellCommand
{
    [Parameter(Position = 0, Mandatory = true, ValueFromPipeline = true), AllowEmptyString, Alias("Msg")]
    public string Message { get; set; } = "";

    protected override void ProcessRecord()
    {
        var host = Context.Host;

        host.UIX.WriteInfoLine(Message);
    }
}