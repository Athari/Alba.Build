using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Management.Automation.Language;
using System.Management.Automation.Runspaces;
using System.Reflection;
using System.Security;
using System.Text;
using Microsoft.Build.Framework;

namespace Alba.Build.PowerShell;

internal class PowerShellHost : PSHost
{
    private static readonly Size DefaultBufferSize = new(200, 10);
    private static readonly string Endl = Environment.NewLine;

    private readonly BuildTask _task;
    private readonly PSShell _shell;

    public override CultureInfo CurrentCulture => Thread.CurrentThread.CurrentCulture;

    public override CultureInfo CurrentUICulture => Thread.CurrentThread.CurrentUICulture;

    public override Guid InstanceId { get; } = Guid.NewGuid();

    public override string Name => "MSBuild";

    public override PowerShellHostUserInterface UI { get; }

    public PowerShellHost(BuildTask task, PSShell shell)
    {
        _task = task;
        _shell = shell;
        UI = new(this);
    }

    public static void RunBuildTask(BuildTask task, Action<PSShell, PowerShellHost> action)
    {
        Environment.SetEnvironmentVariable("PSExecutionPolicyPreference", "Bypass");

        using var shell = PSShell.Create();
        var host = new PowerShellHost(task, shell);
        using var runspace = RunspaceFactory.CreateRunspace(host);

        Runspace.DefaultRunspace = runspace;
        shell.Runspace = runspace;
        shell.Streams.Error.DataAdded +=
            [SuppressMessage("ReSharper", "AccessToDisposedClosure")](_, args) =>
                host.UI.WriteError(shell.Streams.Error[args.Index]);

        runspace.Open();
        try {
            action(shell, host);
            host.UI.Flush();
        }
        catch (RuntimeException e) {
            if (e.ErrorRecord == null)
                throw;
            host.UI.WriteError(e.ErrorRecord);
        }
    }

    public override Version Version =>
        TryGetVersion((AssemblyVersionAttribute a) => a.Version)
     ?? TryGetVersion((AssemblyFileVersionAttribute a) => a.Version)
     ?? TryGetVersion((AssemblyInformationalVersionAttribute a) => a.InformationalVersion)
     ?? new(0, 1, 0, 0);

    private Version? TryGetVersion<T>(Func<T, string> getter) where T : Attribute
    {
        var attr = GetType().Assembly.GetCustomAttribute<T>();
        return attr != null && Version.TryParse(getter(attr), out var ver) ? ver : null;
    }

    public override void NotifyBeginApplication() { }

    public override void NotifyEndApplication() { }

    public override void EnterNestedPrompt() { }

    public override void ExitNestedPrompt() { }

    public override void SetShouldExit(int exitCode) { }

    internal class PowerShellHostUserInterface(PowerShellHost host) : PSHostUserInterface
    {
        private readonly StringBuilder _buffer = new();

        public override PowerShellHostRawUserInterface RawUI { get; } = new(host);

        public override Dictionary<string, PSObject> Prompt(string caption, string message, Collection<FieldDescription> descriptions) => throw NonInteractive();
        public override int PromptForChoice(string caption, string message, Collection<ChoiceDescription> choices, int defaultChoice) => throw NonInteractive();
        public override PSCredential PromptForCredential(string caption, string message, string userName, string targetName) => throw NonInteractive();
        public override PSCredential PromptForCredential(string caption, string message, string userName, string targetName, PSCredentialTypes allowedCredentialTypes, PSCredentialUIOptions options) => throw NonInteractive();
        public override string ReadLine() => throw NonInteractive();
        public override SecureString ReadLineAsSecureString() => throw NonInteractive();

        public override void Write(ConsoleColor foregroundColor, ConsoleColor backgroundColor, string message)
        {
            Write(message);
        }

        public override void Write(string message)
        {
            var lastNewLine = message.LastIndexOf('\n');
            if (lastNewLine == -1) {
                _buffer.Append(message);
                return;
            }

            var logMessage = _buffer + message[..lastNewLine].TrimEnd('\r', '\n');
            if (logMessage.Length > 0)
                host._task.Log.LogMessage(MessageImportance.High, logMessage);

            var remainder = message[(lastNewLine + 1)..];
            _buffer.Clear();
            _buffer.Append(remainder);
        }

        public override void WriteLine(string message)
        {
            Write($"{message}{Endl}");
        }

        public override void WriteDebugLine(string message)
        {
            host._task.Log.LogMessage(MessageImportance.Low, message);
        }

        public override void WriteVerboseLine(string message)
        {
            host._task.Log.LogMessage(MessageImportance.Low, message);
        }

        public override void WriteProgress(long sourceId, ProgressRecord record)
        {
            // TODO: WriteProgress
        }

        public override void WriteWarningLine(string message)
        {
            var stack = GetCallStack();
            host._task.Log.LogWarning(
                LogMessages.Category, LogMessages.WriteHostWarning, null,
                stack.File, stack.LineNumber, stack.ColumnNumber, stack.EndLineNumber, stack.EndColumnNumber,
                message);
        }

        public override void WriteErrorLine(string message)
        {
            var stack = GetCallStack();
            host._task.Log.LogError(
                LogMessages.Category, LogMessages.WriteHostError, null,
                stack.File, stack.LineNumber, stack.ColumnNumber, stack.EndLineNumber, stack.EndColumnNumber,
                message);
        }

        public void WriteError(ErrorRecord error)
        {
            var stack = new CallStack(error);
            host._task.Log.LogError(
                LogMessages.Category, LogMessages.ErrorRecord, null,
                stack.File, stack.LineNumber, stack.ColumnNumber, stack.EndLineNumber, stack.EndColumnNumber,
                $"{error}{Endl}{error.ScriptStackTrace}");
        }

        public void WriteError(ParseError error)
        {
            var stack = new CallStack(error);
            host._task.Log.LogError(
                LogMessages.Category, LogMessages.ParseError, null,
                stack.File, stack.LineNumber, stack.ColumnNumber, stack.EndLineNumber, stack.EndColumnNumber,
                $"{error}");
        }

        private CallStack GetCallStack()
        {
            using var ps = host._shell.CreateNestedPowerShell();
            ps.AddCommand("Get-PSCallStack");
            return new(ps.Invoke<CallStackFrame>().FirstOrDefault());
        }

        public void Flush()
        {
            _buffer.AppendLine();
        }
    }

    internal class PowerShellHostRawUserInterface(PowerShellHost host) : PSHostRawUserInterface
    {
        public override ConsoleColor BackgroundColor { get; set; } = ConsoleColor.Black;
        public override ConsoleColor ForegroundColor { get; set; } = ConsoleColor.White;
        public override Size BufferSize { get; set; } = DefaultBufferSize;
        public override Size WindowSize { get; set; } = DefaultBufferSize;
        public override Size MaxWindowSize => DefaultBufferSize;
        public override Size MaxPhysicalWindowSize => DefaultBufferSize;
        public override Coordinates CursorPosition { get; set; } = new(0, 0);
        public override Coordinates WindowPosition { get; set; } = new(0, 0);
        public override int CursorSize { get; set; } = 1;
        public override string WindowTitle { get; set; } = host.Name;
        public override bool KeyAvailable => false;

        public override KeyInfo ReadKey(ReadKeyOptions options) => throw NonInteractive();
        public override void FlushInputBuffer() => throw NonInteractive();
        public override BufferCell[,] GetBufferContents(Rectangle rectangle) => throw RawNotSupported();
        public override void ScrollBufferContents(Rectangle source, Coordinates destination, Rectangle clip, BufferCell fill) => throw RawNotSupported();
        public override void SetBufferContents(Coordinates origin, BufferCell[,] contents) => throw RawNotSupported();
        public override void SetBufferContents(Rectangle rectangle, BufferCell fill) => throw RawNotSupported();
    }

    private static InvalidOperationException NonInteractive() =>
        new("Interaction with user is not supported from MSBuild tasks.");

    private static NotSupportedException RawNotSupported() =>
        new("Raw user interface is not supported from MSBuild tasks.");

    private class CallStack
    {
        public int LineNumber { get; }
        public int ColumnNumber { get; }
        public int EndLineNumber { get; }
        public int EndColumnNumber { get; }
        public string File { get; }

        internal CallStack(IScriptExtent? position, string? scriptName)
        {
            LineNumber = position?.StartLineNumber ?? 0;
            ColumnNumber = position?.StartColumnNumber ?? 0;
            EndLineNumber = position?.EndLineNumber ?? 0;
            EndColumnNumber = position?.EndColumnNumber ?? 0;
            File = scriptName ?? "";
        }

        internal CallStack(CallStackFrame? frame) : this(frame?.Position, frame?.ScriptName) { }

        // TODO: Parse errors smarter, see https://github.com/rafd123/PowerBridge/blob/master/src/PowerBridge/Internal/LogEntryInfo.cs
        internal CallStack(ErrorRecord error) : this(new CallStackFrame(error.InvocationInfo)) { }

        public CallStack(ParseError error) : this(error.Extent, null) { }
    }
}