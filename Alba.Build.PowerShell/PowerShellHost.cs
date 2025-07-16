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
using Alba.Build.PowerShell.Commands;
using JetBrains.Annotations;
using Microsoft.PowerShell;

namespace Alba.Build.PowerShell;

internal class PowerShellHost : PSHost
{
    private static readonly Size DefaultBufferSize = new(200, 10);
    private static readonly string Endl = Environment.NewLine;

    private static readonly List<Type> Commands = [
        typeof(WriteBuildMessageCommand),
        typeof(WriteInfoCommand),
    ];

    private PowerShellTaskContext _ctx = PowerShellTaskContext.Invalid;

    public PowerShellHostUserInterface UIX { get; private set; } = null!;

    public override Guid InstanceId { get; } = Guid.NewGuid();

    public override PSObject PrivateData => new(_ctx);

    private PowerShellHost() { }

    public static bool RunBuildTask(BuildTask task, Func<PowerShellTaskContext, bool> action)
    {
        //Environment.SetEnvironmentVariable("PSExecutionPolicyPreference", nameof(ExecutionPolicy.Bypass));

        using var ps = PSShell.Create();
        var host = new PowerShellHost();
        host._ctx = new(task, ps, host);
        host.UIX = new(host._ctx);
        using var runspace = RunspaceFactory.CreateRunspace(host, CreateSession());

        Runspace.DefaultRunspace = runspace;
        ps.Runspace = runspace;
        ps.Streams.Error.DataAdded +=
            [SuppressMessage("ReSharper", "AccessToDisposedClosure")](_, args) =>
                host.UIX.WriteError(ps.Streams.Error[args.Index]);

        runspace.Open();
        try {
            //ps.SetExecutionPolicy(ExecutionPolicyScope.Process, ExecutionPolicy.Bypass);
            return action(host._ctx);
        }
        catch (RuntimeException e) {
            host.UIX.LogException(LogLevel.Error, e);
            return false;
        }
        finally {
            host.UIX.Flush();
        }
    }

    private static InitialSessionState CreateSession()
    {
        var session = InitialSessionState.CreateDefault2();
        // TODO session.EnvironmentVariables
        session.ExecutionPolicy = ExecutionPolicy.Bypass;
        session.Commands.Add(Commands.Select(c => new SessionStateCmdletEntry(
            c.GetCustomAttribute<CmdletAttribute>()!.GetName(), c, null)));
        //session.Formats.Add([ 
        //    new(new ExtendedTypeDefinition("typename", [
        //        new("name", new TableControl(
        //            new([
        //                new(Alignment.Center, new("val", DisplayEntryValueType.Property)),
        //            ]) {
        //                Wrap = true,
        //            },
        //            [
        //                new("label", 20, Alignment.Right),
        //            ]
        //        )),
        //    ])),
        //    new(new FormatTable([
        //        "file1",
        //    ])),
        //    new("file2"),
        //]);
        //session.ThreadOptions = PSThreadOptions.UseCurrentThread;
        //session.Providers.Add([
        //    new("", typeof(int), null),
        //]);
        //session.Variables.Add([
        //    new("name", 10, "descr", ScopedItemOptions.Constant),
        //]);
        return session;
    }

    public override PSHostUserInterface UI => UIX;

    public override CultureInfo CurrentCulture => Thread.CurrentThread.CurrentCulture;
    public override CultureInfo CurrentUICulture => Thread.CurrentThread.CurrentUICulture;
    public override string Name => "MSBuild";

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

    internal class PowerShellHostUserInterface(PowerShellTaskContext ctx) : PSHostUserInterface
    {
        private readonly StringBuilder _buffer = new();

        public PowerShellHostRawUserInterface RawUIX { get; } = new(ctx);
        public override PSHostRawUserInterface RawUI => RawUIX;

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
                LogMessage(LogLevel.MessageHigh, logMessage,
                    ErrorCat.Build, ErrorCode.WriteHostMessage, CallStack.Empty);

            var remainder = message[(lastNewLine + 1)..];
            _buffer.Clear();
            _buffer.Append(remainder);
        }

        public override void WriteLine(string message) =>
            Write($"{message}{Endl}");

        public override void WriteDebugLine(string message) =>
            LogMessage(LogLevel.MessageLow, message,
                ErrorCat.Build, ErrorCode.WriteHostDebug, CallStack.Empty);

        public override void WriteVerboseLine(string message) =>
            LogMessage(LogLevel.MessageLow, message,
                ErrorCat.Build, ErrorCode.WriteHostVerbose, CallStack.Empty);

        public override void WriteProgress(long sourceId, ProgressRecord record)
        {
            // TODO: WriteProgress
        }

        public void WriteInfoLine(string message) =>
            LogMessage(LogLevel.Info, message,
                ErrorCat.Build, ErrorCode.WriteHostInfo, new(ctx.Shell.GetCallStack()));

        public override void WriteWarningLine(string message) =>
            LogMessage(LogLevel.Warning, message,
                ErrorCat.Build, ErrorCode.WriteHostWarning, new(ctx.Shell.GetCallStack()));

        public override void WriteErrorLine(string message) =>
            LogMessage(LogLevel.Error, message,
                ErrorCat.Build, ErrorCode.WriteHostError, new(ctx.Shell.GetCallStack()));

        public void WriteError(ErrorRecord error)
        {
            var message = error.ErrorDetails?.Message.NullIfEmpty() ?? error.Exception?.Message ?? error.ToString();
            var text = $"{message}{Endl}{error.ScriptStackTrace}";
            if (error.Exception != null)
                LogException(LogLevel.Error, error.Exception, true, text, unwrapErrorRecord: false);
            else
                LogMessage(LogLevel.Error, text,
                    ErrorCat.Build, ErrorCode.ErrorRecord, new(error));
        }

        public void WriteError(ParseError error) =>
            LogMessage(LogLevel.Error, $"{error}",
                ErrorCat.Build, ErrorCode.ParseError, new(error));

        public bool LogException(
            LogLevel level, Exception e, bool withStackTrace = true, string? message = null,
            [ValueProvider(ErrorProvider.Cat)] string? category = null,
            [ValueProvider(ErrorProvider.Code)] string? code = null,
            bool unwrapErrorRecord = true, CallStack? stack = null, string? helpLink = null)
        {
            //Exts.LaunchDebugger();
            if (unwrapErrorRecord && e is RuntimeException { ErrorRecord: var error }) {
                WriteError(error);
                return false;
            }
            var text = (message != null ? $"{message} " : "")
              + e.Message
              + (withStackTrace && e.StackTrace?.Length > 0 ? $"{Endl}{e.StackTrace}" : "");
            LogMessage(level, text,
                category ?? ErrorCat.Build, code ?? GetExceptionErrorCode(e),
                stack ?? CallStack.Empty, helpLink);
            return false;
        }

        public void LogMessage(
            LogLevel level, string message,
            [ValueProvider(ErrorProvider.Cat)] string category,
            [ValueProvider(ErrorProvider.Code)] string code,
            CallStack stack, string? helpLink = null)
        {
            (stack, message) =
                stack.IsEmpty ? (CallStack.Empty, message)
                : stack.IsFileEmpty ? (CallStack.Empty, stack.ToShortBuildFormat(message))
                : (stack, message);
            //var text = stack.ToFullBuildFormat(level, message, category, code);
            switch (level) {
                case LogLevel.MessageLow:
                case LogLevel.MessageNormal:
                case LogLevel.MessageHigh:
                    ctx.Task.Log.LogMessage(
                        category, code, null,
                        stack.File, stack.Line, stack.Column, stack.EndLine, stack.EndColumn,
                        level.ToImportance(), message);
                    return;
                case LogLevel.Warning:
                    ctx.Task.Log.LogWarning(
                        category, code, null, helpLink,
                        stack.File, stack.Line, stack.Column, stack.EndLine, stack.EndColumn,
                        message);
                    break;
                case LogLevel.Error:
                    ctx.Task.Log.LogError(
                        category, code, null, helpLink,
                        stack.File, stack.Line, stack.Column, stack.EndLine, stack.EndColumn,
                        message);
                    break;
                case LogLevel.Info:
                    ctx.Task.Log.LogCriticalMessage(
                        category, code, null,
                        stack.File, stack.Line, stack.Column, stack.EndLine, stack.EndColumn,
                        message);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(level), level, null);
            }
        }

        public void Flush()
        {
            _buffer.AppendLine();
        }
    }

    internal class PowerShellHostRawUserInterface(PowerShellTaskContext ctx) : PSHostRawUserInterface
    {
        public override ConsoleColor BackgroundColor { get; set; } = ConsoleColor.Black;
        public override ConsoleColor ForegroundColor { get; set; } = ConsoleColor.White;
        public override Size BufferSize { get; set; } = DefaultBufferSize;
        public override Size WindowSize { get; set; } = DefaultBufferSize;
        public override Size MaxWindowSize => DefaultBufferSize;
        public override Size MaxPhysicalWindowSize => DefaultBufferSize;
        public override Coordinates CursorPosition { get; set; } = new(0, 0);
        public override Coordinates WindowPosition { get; set; } = new(0, 0);
        public override int CursorSize { get; set; } = 25;
        public override string WindowTitle { get; set; } = ctx.Host.Name;
        public override bool KeyAvailable => false;

        public override KeyInfo ReadKey(ReadKeyOptions options) => throw NonInteractive();
        public override void FlushInputBuffer() => throw NonInteractive();
        public override BufferCell[,] GetBufferContents(Rectangle rectangle) => throw RawNotSupported();
        public override void ScrollBufferContents(Rectangle source, Coordinates destination, Rectangle clip, BufferCell fill) => throw RawNotSupported();
        public override void SetBufferContents(Coordinates origin, BufferCell[,] contents) => throw RawNotSupported();
        public override void SetBufferContents(Rectangle rectangle, BufferCell fill) => throw RawNotSupported();
    }

    private static NonInteractiveException NonInteractive() => new();

    private static RawOutputException RawNotSupported() => new();

    [SuppressMessage("ReSharper", "RedundantTypeCheckInPattern")]
    private static string GetExceptionErrorCode(Exception e) =>
        e switch {
            ArgumentException => ErrorCode.ArgumentError,
            NonInteractiveException => ErrorCode.NonInteractiveError,
            RawOutputException => ErrorCode.RawOutputError,
            AggregateException { InnerExceptions: var exs } => exs
                .Select(GetExceptionErrorCode)
                .FirstOrDefault(c => c != ErrorCode.RuntimeError) ?? ErrorCode.RuntimeError,
            RuntimeException { ErrorRecord.Exception: { } ex } => GetExceptionErrorCode(ex),
            Exception { InnerException: { } ex } => GetExceptionErrorCode(ex),
            RuntimeException => ErrorCode.RuntimeError,
            _ => ErrorCode.InternalError,
        };
}