using System.Management.Automation;
using Microsoft.Build.Framework;

namespace Alba.Build.PowerShell.Commands;

[Cmdlet(VerbsCommunications.Write, "BuildMessage")]
public class WriteBuildMessageCommand : PowerShellCommand
{
    [Parameter(Position = 0, Mandatory = true)]
    public MessageCategory Category { get; set; } = MessageCategory.Message;

    [Parameter(Position = 1, Mandatory = true, ValueFromPipeline = true)]
    public string Message { get; set; } = "";

    [Parameter, Alias(nameof(Exception))]
    public object? Error { get; set; }

    [Parameter]
    public MessageImportance Importance { get; set; } = MessageImportance.Normal;

    [Parameter, Alias(nameof(File), nameof(Path))]
    public string? Origin { get; set; }

    [Parameter]
    public string? Subcategory { get; set; }

    [Parameter, Alias("ErrorCode")]
    public string? Code { get; set; }

    [Parameter, Alias("Link")]
    public string? HelpLink { get; set; }

    protected override void ProcessRecord()
    {
        var host = Context.Host;
        var logLevel = Category.ToLogLevel(Importance);
        var cat = Subcategory ?? ErrorCat.Build;

        switch (Error) {
            case Exception e:
                host.UIX.LogException(logLevel, e, withStackTrace: true, Message,
                    cat, Code ?? ErrorCode.WriteBuildError, unwrapErrorRecord: true, new(Origin), HelpLink);
                break;

            case ErrorRecord error:
                if (error is { Exception: { } inner })
                    host.UIX.LogException(logLevel, inner, withStackTrace: true, Message,
                        cat, Code ?? ErrorCode.WriteBuildError, unwrapErrorRecord: false, new(Origin), HelpLink);
                else
                    host.UIX.LogMessage(logLevel, Message,
                        cat, Code ?? ErrorCode.WriteBuildError, Origin != null ? new(Origin) : new(error), HelpLink);
                break;

            case null:
                host.UIX.LogMessage(logLevel, Message, cat, Code ?? ErrorCode.WriteBuildMessage, new(Origin), HelpLink);
                break;

            default:
                throw new ArgumentException($"Unexpected Error type: {Error.GetType().FullName}", nameof(Error));
        }
    }
}