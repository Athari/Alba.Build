using System.Management.Automation;

namespace Alba.Build.PowerShell.Commands;

[Cmdlet(VerbsCommunications.Write, nameof(LogLevel.Info))]
public class WriteInfoCommand : PSBuildCommand
{
    [Parameter(Position = 0, Mandatory = true, ValueFromPipeline = true), AllowEmptyString, Alias("Msg")]
    public string Message { get; set; } = "";

    protected override void ProcessRecord()
    {
        var host = Ctx.Host;

        host.UI.WriteInfoLine(Message);
    }
}