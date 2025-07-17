using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Management.Automation.Language;
using System.Management.Automation.Runspaces;
using Microsoft.Build.Framework;

namespace Alba.Build.PowerShell.Tasks;

public partial class ExecPowerShell : BuildTask
{
    private bool _isScriptSpecified;
    private bool _isFileSpecified;

    [field: MaybeNull]
    public string Script { get; set => SetSpecified(out field, value, out _isScriptSpecified); }

    [field: MaybeNull]
    public string File { get; set => SetSpecified(out field, value, out _isFileSpecified); }

    public string[]? EnvironmentVariables { get; set; }

    public string? WorkingDirectory { get; set; }

    public MessageImportance DefaultMessageImportance { get; set; } = MessageImportance.High;

    public bool OutputErrorStackTraces { get; set; } = true;

    public bool OutputExceptionStackTraces { get; set; } = false;

    public bool LaunchDebugger { get; set; }

    internal Dictionary<string, object> EnvironmentDictionary { get; private set; } = [ ];

    public override bool Execute()
    {
        if (LaunchDebugger)
            Exts.LaunchDebugger();

        EnvironmentDictionary = (EnvironmentVariables ?? [ ])
            .Select(v => v.Split([ '=' ], 2))
            .ToDictionarySafeOIC(v => v[0].Trim(), object (v) => v.ElementAtOrDefault(1)?.Trim() ?? "");
        var args = new ScriptArgs();
        args.CopyFromTask(this);

        var done = PowerShellHost.RunBuildTask(this, ctx => {
            args.Project = BuildEngine6.GetGlobalProperties();
            if (!AddScript(ctx, args))
                return false;

            using var dirFrame = new WorkingDirectoryFrame(WorkingDirectory);
            ctx.Shell.Invoke(null, new PowerShellOutputCollection(ctx));
            args.CopyToTask(this);

            return true;
        });

        return done && !Log.HasLoggedErrors;
    }

    private bool AddScript(PowerShellTaskContext ctx, ScriptArgs args)
    {
        var (_, shell, host) = ctx;
        if (_isScriptSpecified == _isFileSpecified)
            return ctx.Host.UIX.LogException(LogLevel.Error,
                new ArgumentException($"Must specify either {nameof(Script)} or {nameof(File)}."));

        ScriptCommandInfo scriptInfo = null!;
        if (_isScriptSpecified) {
            var scriptAst = Parser.ParseInput(Script, out _, out var errors);
            if (errors?.Length > 0) {
                foreach (var error in errors)
                    host.UIX.WriteError(error);
                return false;
            }
            scriptInfo = new(ctx, scriptAst);
            shell.AddScript(Script);
        }
        else if (_isFileSpecified) {
            var scriptPath = Path.GetFullPath(File);
            var command = new Command(scriptPath, isScript: false);
            scriptInfo = new(ctx, command);
            shell.Commands.AddCommand(command);
            ctx.Task.Log.LogCommandLine(scriptPath);
        }

        foreach ((string name, object? value) in GetNamedParameters(args, scriptInfo))
            shell.AddParameter(name, value);
        if (scriptInfo.Parameters.Count == 0)
            shell.AddArgument(args);
        return true;
    }

    private IDictionary<string, object?> GetNamedParameters(ScriptArgs args, ScriptCommandInfo scriptInfo)
    {
        var env = Environment.GetEnvironmentVariables().OfType<DictionaryEntry>()
            .ToDictionary(e => e.Key as string ?? "", e => e.Value);
        var parameters = new Dictionary<string, object?>();
        foreach (var param in scriptInfo.Parameters) {
            object? value;
            foreach (var alias in param.Aliases)
                if (args.TryGetNamedValue(alias, out value))
                    goto found;
            if (args.TryGetNamedValue(param.Name, out value))
                goto found;
            if (EnvironmentDictionary.TryGetValue(param.Name, out value))
                goto found;
            if (env.TryGetValue(param.Name, out value))
                goto found;
            continue;
            found:
            parameters.Add(param.Name, value);
        }
        return parameters;
    }

    private static void SetSpecified<T>(out T field, T value, out bool isSpecified)
    {
        field = value;
        isSpecified = true;
    }

    private readonly struct WorkingDirectoryFrame : IDisposable
    {
        private readonly string? _prevDir;

        public WorkingDirectoryFrame(string? newDir)
        {
            if (newDir == null)
                return;
            _prevDir = Directory.GetCurrentDirectory();
            Directory.SetCurrentDirectory(newDir);
        }

        public void Dispose()
        {
            if (_prevDir == null)
                return;
            Directory.SetCurrentDirectory(_prevDir);
        }
    }
}