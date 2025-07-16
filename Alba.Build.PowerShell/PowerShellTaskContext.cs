using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Alba.Build.PowerShell.Tasks;

namespace Alba.Build.PowerShell;

internal class PowerShellTaskContext(ExecPowerShell task, PSShell shell, PowerShellHost host)
{
    public ExecPowerShell Task { get => Get(field); set; } = task;
    public PSShell Shell { get => Get(field); set; } = shell;
    public PowerShellHost Host { get => Get(field); set; } = host;

    public void Deconstruct(out ExecPowerShell task, out PSShell shell, out PowerShellHost host) =>
        (task, shell, host) = (Task, Shell, Host);

    [field: MaybeNull]
    public static PowerShellTaskContext Invalid => field ??= new InvalidPowerShellTaskContext();

    private class InvalidPowerShellTaskContext() : PowerShellTaskContext(null!, null!, null!);

    private static T Get<T>(T field, [CallerMemberName] string? name = null) =>
        field ?? throw new InvalidOperationException($"{name} not set");
}