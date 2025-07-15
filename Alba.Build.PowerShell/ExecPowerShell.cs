using System.Diagnostics.CodeAnalysis;
using System.Management.Automation.Language;
using System.Management.Automation.Runspaces;

namespace Alba.Build.PowerShell;

public partial class ExecPowerShell : BuildTask
{
    private bool _isScriptSpecified;
    private bool _isFileSpecified;

    [field: MaybeNull]
    public string Script
    {
        get;
        set => SetSpecified(out field, value, out _isScriptSpecified);
    }

    [field: MaybeNull]
    public string File
    {
        get;
        set => SetSpecified(out field, value, out _isFileSpecified);
    }

    public override bool Execute()
    {
        //var s = ast.ParamBlock.Parameters.First()
        //    .Attributes.OfType<AttributeAst>().First()
        //    .PositionalArguments.First()
        //    .FindAll(s => s is StringConstantExpressionAst, false).First()
        //    .SafeGetValue() as string;

        var param = new ScriptParams();
        param.CopyFromTask(this);

        PowerShellHost.RunBuildTask(this, (shell, host) => {
            param.Project = BuildEngine9.GetGlobalProperties();

            if (_isScriptSpecified) {
                var ast = Parser.ParseInput(Script, out _, out var errors);
                if (errors != null) {
                    foreach (var error in errors)
                        host.UIX.WriteError(error);
                    return;
                }

                var script = ast.GetScriptBlock();
                shell.AddScript(script.ToString()).AddArgument(param);
            }
            else if (_isFileSpecified) {
                var command = new Command(Path.GetFullPath(File), isScript: false);
                shell.Commands.AddCommand(command).AddArgument(param);
            }

            shell.Invoke(null, new PowerShellOutputCollection(shell, host));
        });

        return !Log.HasLoggedErrors;
    }

    //private IEnumerable<KeyValuePair<string, string>> GetCommandParameterValuesThatMatchAutoParameters(string commandName)
    //{
    //    var parameterNames = GetDefaultParameterSetParameterNames(commandName);
    //    var autoParametersLookup = Params1
    //        .Select(x => new {
    //            ParameterName = x.ItemSpec,
    //            ParameterValue = x.GetMetadata("Value"),
    //        })
    //        .Where(x => !string.IsNullOrEmpty(x.ParameterValue))
    //        .ToLookup(x => x.ParameterName, x => x.ParameterValue, StringComparer.OrdinalIgnoreCase)
    //        .ToDictionary(x => x.Key, x => x.Last());

    //    foreach (var parameterName in parameterNames)
    //        if (autoParametersLookup.TryGetValue(parameterName, out string parameterValue))
    //            yield return new(parameterName, parameterValue);
    //}

    //private string[] GetDefaultParameterSetParameterNames(string commandName)
    //{
    //    using var ps = PSShell.Create(RunspaceMode.NewRunspace);
    //    var commandInfo = ps.AddCommand("Get-Command").AddParameter("Name", commandName).Invoke<CommandInfo>().First();
    //    var parameterSet = commandInfo.ParameterSets.Count == 1
    //        ? commandInfo.ParameterSets[0]
    //        : commandInfo.ParameterSets.FirstOrDefault(x => x.IsDefault);
    //    if (parameterSet == null)
    //        throw new InvalidOperationException(commandName + " does not have a default parameter set.");
    //    return parameterSet.Parameters.Select(x => x.Name).ToArray();
    //}

    private static void SetSpecified<T>(out T field, T value, out bool isSpecified)
    {
        field = value;
        isSpecified = true;
    }
}