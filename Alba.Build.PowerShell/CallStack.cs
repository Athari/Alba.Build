using System.Management.Automation;
using System.Management.Automation.Language;

namespace Alba.Build.PowerShell;

internal class CallStack
{
    public static readonly CallStack Empty = new();

    public int Line { get; }
    public int Column { get; }
    public int EndLine { get; }
    public int EndColumn { get; }
    public string File { get; }

    public bool IsFileEmpty => File.IsNullOrEmpty();
    public bool IsEmpty => IsFileEmpty && Line == 0;

    private CallStack(IScriptExtent? position, string? scriptName)
    {
        Line = position?.StartLineNumber ?? 0;
        Column = position?.StartColumnNumber ?? 0;
        EndLine = position?.EndLineNumber ?? 0;
        EndColumn = position?.EndColumnNumber ?? 0;
        File = scriptName ?? "";
    }

    internal CallStack(CallStackFrame? frame) : this(frame?.Position, frame?.ScriptName) { }

    // TODO: Parse errors smarter, see https://github.com/rafd123/PowerBridge/blob/master/src/PowerBridge/Internal/LogEntryInfo.cs
    internal CallStack(ErrorRecord error) : this(new CallStackFrame(error.InvocationInfo)) { }

    public CallStack(ParseError error) : this(error.Extent, null) { }

    public CallStack(string? file) => File = file ?? "";

    public CallStack() : this(null, null) { }

    /// <remarks>https://learn.microsoft.com/en-us/visualstudio/msbuild/msbuild-diagnostic-format-for-tasks</remarks>
    public string ToFullBuildFormat(LogLevel level, string message, string cat, string code) =>
        $"{FormatFile()}{FormatPosition()}: {cat} {FormatCategory(level)} {code}: {message}";

    public string ToShortBuildFormat(string message) =>
        IsEmpty ? message : $"{FormatFile()}{FormatPosition()}: {message}";

    private string FormatFile() => File.NullIfEmpty() ?? "<ScriptBlock>";
    private static string FormatCategory(LogLevel level) => level.ToMessageCategory().ToString().ToLowerInvariant();

    private string FormatPosition() =>
        Line == 0 ? ""
        : Column == 0 ? EndLine == 0 ? $"({Line})" : $"({Line}-{EndLine})"
        : EndColumn == 0 ? $"({Line},{Column})"
        : EndColumn == 0 ? $"({Line},{Column}-{EndColumn})"
        : $"({Line},{Column},{EndLine},{EndColumn})";
}