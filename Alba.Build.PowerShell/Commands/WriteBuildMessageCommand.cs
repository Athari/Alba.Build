using System.Management.Automation;
using Microsoft.Build.Framework;

namespace Alba.Build.PowerShell.Commands;

[Cmdlet(VerbsCommunications.Write, "BuildMessage")]
public class WriteBuildMessageCommand : PSBuildCommand
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

    [Parameter, Alias("StackTrace")]
    public bool? WithStackTrace { get; set; }

    protected override void ProcessRecord()
    {
        var host = Ctx.Host;
        var logLevel = Category.ToLogLevel(Importance);
        var cat = Subcategory ?? ErrorCat.Build;

        switch (Error) {
            case Exception e:
                host.UI.LogException(logLevel, e, WithStackTrace, Message,
                    cat, Code ?? ErrorCode.WriteBuildError, unwrapErrorRecord: true, new(Origin), HelpLink);
                break;

            case ErrorRecord error:
                string text = host.UI.GetErrorRecordMessageText(error, WithStackTrace);
                if (error is { Exception: { } inner })
                    host.UI.LogException(logLevel, inner, WithStackTrace, text,
                        cat, Code ?? ErrorCode.WriteBuildError, unwrapErrorRecord: false, new(Origin), HelpLink);
                else
                    host.UI.LogMessage(logLevel, text,
                        cat, Code ?? ErrorCode.WriteBuildError, Origin != null ? new(Origin) : new(error), HelpLink);
                break;

            case null:
                host.UI.LogMessage(logLevel, Message, cat, Code ?? ErrorCode.WriteBuildMessage, new(Origin), HelpLink);
                break;

            default:
                throw new ArgumentException($"Unexpected Error type: {Error.GetType().FullName}", nameof(Error));
        }
    }
}