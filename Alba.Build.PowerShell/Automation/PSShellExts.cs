using System.Management.Automation;
using Microsoft.PowerShell;

namespace Alba.Build.PowerShell;

internal static class PSShellExts
{
    public static void SetExecutionPolicy(this PSShell @this,
        ExecutionPolicyScope scope, ExecutionPolicy policy,
        ActionPreference errorAction = ActionPreference.Stop, bool force = false) =>
        @this.AddCommand("Set-ExecutionPolicy")
            .AddParameter("ExecutionPolicy", policy)
            .AddParameter("Scope", scope)
            .AddParameter("ErrorAction", errorAction)
            .AddParameter("Force", force)
            .Invoke();

    public static CallStackFrame? GetCallStack(this PSShell @this) =>
        @this.GetResultOrDefaultNested<CallStackFrame>(ps =>
            ps.AddCommand("Get-PSCallStack"));

    public static string? GetOutString(this PSShell @this, object o) =>
        @this.GetResultOrDefaultNested<string>(ps =>
            ps.AddCommand("Out-String").AddParameter("InputObject", o));

    public static CommandInfo GetCommandInfo(this PSShell @this, string name) =>
        @this.GetResultNested<CommandInfo>(ps =>
            ps.AddCommand("Get-Command").AddParameter("Name", name));

    public static T GetResultNested<T>(this PSShell @this, Func<PSShell, PSShell> call) =>
        @this.CreateNestedPowerShell().GetResult<T>(call);

    public static T? GetResultOrDefaultNested<T>(this PSShell @this, Func<PSShell, PSShell> call) =>
        @this.CreateNestedPowerShell().GetResultOrDefault<T>(call);

    private static T GetResult<T>(this PSShell @this, Func<PSShell, PSShell> call)
    {
        using var _ = @this;
        return call(@this).Invoke<T>().First();
    }

    private static T? GetResultOrDefault<T>(this PSShell @this, Func<PSShell, PSShell> call)
    {
        using var _ = @this;
        return call(@this).Invoke<T>().FirstOrDefault();
    }
}