using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Management.Automation.Language;
using System.Management.Automation.Runspaces;
using System.Reflection;
using System.Security;
using System.Text;
using Alba.Build.PowerShell.Commands;
using Alba.Build.PowerShell.Tasks;
using JetBrains.Annotations;
using Microsoft.PowerShell;

namespace Alba.Build.PowerShell;

internal class PSBuildHost : PSBaseHost
{
    private static readonly List<Type> Commands = [
        typeof(WriteBuildMessageCommand),
        typeof(WriteInfoCommand),
    ];

    protected new PSBuildTaskContext Ctx
    {
        get => (PSBuildTaskContext)base.Ctx;
        private protected set => base.Ctx = value;
    }

    public new PSBuildHostUI UI
    {
        get => (PSBuildHostUI)_UI;
        private protected set => _UI = value;
    }

    public new PSBuildHostRawUI RawUI
    {
        get => (PSBuildHostRawUI)base.RawUI;
        private protected set => base.RawUI = value;
    }

    private protected PSBuildHost() { }

    public PSBuildHost(PSShell shell, ExecPowerShellTask task)
    {
        Ctx = new(shell, this, task);
        UI = new(Ctx);
        RawUI = new(Ctx);
    }

    public static bool RunBuildTask(Func<PSShell, PSBuildHost> createHost, Func<PSBuildTaskContext, bool> action)
    {
        using var ps = PSShell.Create();
        var host = createHost(ps);

        var session = InitialSessionState.CreateDefault2();
        host.InitSession(session);
        using var runspace = RunspaceFactory.CreateRunspace(host, session);
        host.InitRunspace(runspace);

        Runspace.DefaultRunspace = runspace;
        ps.Runspace = runspace;
        ps.Streams.Error.DataAdded +=
            [SuppressMessage("ReSharper", "AccessToDisposedClosure")](_, args) =>
                host.UI.WriteError(ps.Streams.Error[args.Index]);

        runspace.Open();
        try {
            //ps.SetExecutionPolicy(ExecutionPolicyScope.Process, ExecutionPolicy.Bypass);
            return action(host.Ctx);
        }
        catch (RuntimeException e) {
            host.UI.LogException(LogLevel.Error, e);
            return false;
        }
        finally {
            host.UI.Flush();
        }
    }

    protected virtual void InitSession(InitialSessionState session)
    {
        session.ExecutionPolicy = ExecutionPolicy.Bypass;
        session.Commands.Add(Commands
            .Select(c => new SessionStateCmdletEntry(c.GetCustomAttribute<CmdletAttribute>()!.GetName(), c, null)));
        if (Ctx.Task.EnvironmentVariables != null)
            session.EnvironmentVariables.Add(Ctx.Task.EnvironmentDictionary
                .Select(v => new SessionStateVariableEntry(v.Key, v.Value, null)));
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
    }

    protected virtual void InitRunspace(Runspace runspace) { }

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

    protected internal class PSBuildHostUI(PSBuildTaskContext ctx) : PSBaseHostUI(ctx)
    {
        private readonly StringBuilder _buffer = new();

        protected new PSBuildTaskContext Ctx
        {
            get => (PSBuildTaskContext)base.Ctx;
            private protected set => base.Ctx = value;
        }

        public override Dictionary<string, PSObject> Prompt(string caption, string message, Collection<FieldDescription> descriptions) => throw NonInteractive();
        public override int PromptForChoice(string caption, string message, Collection<ChoiceDescription> choices, int defaultChoice) => throw NonInteractive();
        public override Collection<int> PromptForChoice(string caption, string message, Collection<ChoiceDescription> choices, IEnumerable<int> defaultChoices) => throw NonInteractive();
        public override PSCredential PromptForCredential(string caption, string message, string userName, string targetName) => throw NonInteractive();
        public override PSCredential PromptForCredential(string caption, string message, string userName, string targetName, PSCredentialTypes allowedCredentialTypes, PSCredentialUIOptions options) => throw NonInteractive();
        public override string ReadLine() => throw NonInteractive();
        public override SecureString ReadLineAsSecureString() => throw NonInteractive();

        public override void Write(string message)
        {
            var lastNewLine = message.LastIndexOf('\n');
            if (lastNewLine == -1) {
                _buffer.Append(message);
                return;
            }

            var logMessage = _buffer + message[..lastNewLine].TrimEnd('\r', '\n');
            if (logMessage.Length > 0)
                LogMessage(Ctx.Task.DefaultMessageImportance.ToLogLevel(), logMessage,
                    ErrorCat.Build, ErrorCode.WriteHostMessage, PSBuildCallStack.Empty);

            var remainder = message[(lastNewLine + 1)..];
            _buffer.Clear();
            _buffer.Append(remainder);
        }

        public override void WriteDebugLine(string message) =>
            LogMessage(LogLevel.MessageLow, message,
                ErrorCat.Build, ErrorCode.WriteHostDebug, PSBuildCallStack.Empty);

        public override void WriteVerboseLine(string message) =>
            LogMessage(LogLevel.MessageLow, message,
                ErrorCat.Build, ErrorCode.WriteHostVerbose, PSBuildCallStack.Empty);

        public override void WriteProgress(long sourceId, ProgressRecord record)
        {
            // TODO: WriteProgress
        }

        public void WriteInfoLine(string message) =>
            LogMessage(LogLevel.Info, message,
                ErrorCat.Build, ErrorCode.WriteHostInfo, new(Ctx.Shell.GetCallStack()));

        public override void WriteWarningLine(string message) =>
            LogMessage(LogLevel.Warning, message,
                ErrorCat.Build, ErrorCode.WriteHostWarning, new(Ctx.Shell.GetCallStack()));

        public override void WriteErrorLine(string message) =>
            LogMessage(LogLevel.Error, message,
                ErrorCat.Build, ErrorCode.WriteHostError, new(Ctx.Shell.GetCallStack()));

        public void WriteError(ErrorRecord error, bool? withStackTrace = null)
        {
            string text = GetErrorRecordMessageText(error, withStackTrace);
            if (error.Exception != null)
                LogException(LogLevel.Error, error.Exception, withStackTrace, text, unwrapErrorRecord: false);
            else
                LogMessage(LogLevel.Error, text, ErrorCat.Build, ErrorCode.ErrorRecord, new(error));
        }

        public void WriteError(ParseError error) =>
            LogMessage(LogLevel.Error, $"{error}",
                ErrorCat.Build, ErrorCode.ParseError, new(error));

        public bool LogException(
            LogLevel level, Exception e, bool? withStackTrace = null, string? message = null,
            [ValueProvider(ErrorProvider.Cat)] string? category = null,
            [ValueProvider(ErrorProvider.Code)] string? code = null,
            bool unwrapErrorRecord = true, PSBuildCallStack? stack = null, string? helpLink = null)
        {
            //Exts.LaunchDebugger();
            if (unwrapErrorRecord && e is RuntimeException { ErrorRecord: var error }) {
                WriteError(error, withStackTrace);
                return false;
            }
            var text = (message != null ? $"{message} " : "")
              + e.Message
              + ((withStackTrace ?? Ctx.Task.OutputExceptionStackTraces) && e.StackTrace?.Length > 0 ? $"{Endl}{e.StackTrace}" : "");
            LogMessage(level, text,
                category ?? ErrorCat.Build, code ?? GetExceptionErrorCode(e),
                stack ?? PSBuildCallStack.Empty, helpLink);
            return false;
        }

        public void LogMessage(
            LogLevel level, string message,
            [ValueProvider(ErrorProvider.Cat)] string category,
            [ValueProvider(ErrorProvider.Code)] string code,
            PSBuildCallStack stack, string? helpLink = null)
        {
            (stack, message) =
                stack.IsEmpty ? (PSBuildCallStack.Empty, message)
                : stack.IsFileEmpty ? (PSBuildCallStack.Empty, stack.ToShortBuildFormat(message))
                : (stack, message);
            //var text = stack.ToFullBuildFormat(level, message, category, code);
            switch (level) {
                case LogLevel.MessageLow:
                case LogLevel.MessageNormal:
                case LogLevel.MessageHigh:
                    Ctx.Task.Log.LogMessage(
                        category, code, null,
                        stack.File, stack.Line, stack.Column, stack.EndLine, stack.EndColumn,
                        level.ToImportance(), message);
                    return;
                case LogLevel.Warning:
                    Ctx.Task.Log.LogWarning(
                        category, code, null, helpLink,
                        stack.File, stack.Line, stack.Column, stack.EndLine, stack.EndColumn,
                        message);
                    break;
                case LogLevel.Error:
                    Ctx.Task.Log.LogError(
                        category, code, null, helpLink,
                        stack.File, stack.Line, stack.Column, stack.EndLine, stack.EndColumn,
                        message);
                    break;
                case LogLevel.Info:
                    Ctx.Task.Log.LogCriticalMessage(
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

        public string GetErrorRecordMessageText(ErrorRecord error, bool? withStackTrace)
        {
            var message = error.ErrorDetails?.Message.NullIfEmpty() ?? error.Exception?.Message ?? error.ToString();
            return withStackTrace ?? Ctx.Task.OutputErrorStackTraces
                ? $"{message}{Endl}{error.ScriptStackTrace}"
                : message;
        }
    }

    protected internal class PSBuildHostRawUI(PSBuildTaskContext ctx) : PSBaseHostRawUI(ctx)
    {
        protected new PSBuildTaskContext Ctx
        {
            get => (PSBuildTaskContext)base.Ctx;
            private protected set => base.Ctx = value;
        }

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
}