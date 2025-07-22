using System.Diagnostics.CodeAnalysis;
using System.Management.Automation.Language;
using System.Management.Automation.Runspaces;
using Alba.Build.PowerShell.Common;
using Microsoft.Build.Framework;

namespace Alba.Build.PowerShell.Tasks;

public partial class ExecPowerShellTask : BuildTask
{
    private bool _isScriptSpecified;
    private bool _isFileSpecified;

    [field: MaybeNull]
    public string Script {
        get;
        set => SetSpecified(out field, value, out _isScriptSpecified);
    }

    [field: MaybeNull]
    public string File {
        get;
        set => SetSpecified(out field, value, out _isFileSpecified);
    }

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
            Env.LaunchDebugger();

        EnvironmentDictionary = (EnvironmentVariables ?? [ ])
            .Select(v => v.Split([ '=' ], 2))
            .ToDictionarySafeOIC(v => v[0].Trim(), object (v) => v.ElementAtOrDefault(1)?.Trim() ?? "");
        var args = new ScriptArgs();
        args.CopyFromTask(this);

        var done = PSBuildHost.RunBuildTask(CreateHost, ctx => {
            args.Project = BuildEngine6.GetGlobalProperties();
            if (!AddScript(ctx, args))
                return false;

            using var dirFrame = Env.WithCurrentDirectory(WorkingDirectory);
            Run(ctx);
            args.CopyToTask(this);

            return true;
        });

        return done && !Log.HasLoggedErrors;
    }

    private protected virtual PSBuildHost CreateHost(PSShell ps) =>
        new(ps, this);

    private protected virtual void Run(PSBaseTaskContext ctx) =>
        ctx.Shell.Invoke(null, new PSOutputCollection((PSBuildTaskContext)ctx));

    private bool AddScript(PSBuildTaskContext ctx, ScriptArgs args)
    {
        var (shell, host, _) = ctx;
        if (_isScriptSpecified == _isFileSpecified)
            return ctx.Host.UI.LogException(LogLevel.Error,
                new ArgumentException($"Must specify either {nameof(Script)} or {nameof(File)}."));

        ScriptCommandInfo scriptInfo = null!;
        if (_isScriptSpecified) {
            var scriptAst = Parser.ParseInput(Script, out _, out var errors);
            if (errors?.Length > 0) {
                foreach (var error in errors)
                    host.UI.WriteError(error);
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
        var env = Environment.GetEnvironmentVariables().AsTyped<string, object?>();
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
}