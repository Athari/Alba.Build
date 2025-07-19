using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Reflection;

namespace Alba.Build.PowerShell;

internal abstract class PSBaseHost : PSHost
{
    protected static readonly Size DefaultBufferSize = new(200, 10);
    protected static readonly string Endl = Environment.NewLine;

    protected PSBaseTaskContext Ctx { get; set; } = null!;

    [SuppressMessage("Style", "IDE1006:Naming rule violation", Justification =
        "Workaround for the lack of support for covariant returns in .NET Framework 4.8 runtime and lack of overriding " +
        "read-only properties with writable properties; must not accessed outside of direct inheritors.")]
    private protected PSBaseHostUI _UI { get; set; } = null!;

    public sealed override PSHostUserInterface UI => _UI;

    public PSBaseHostRawUI RawUI { get; protected set; } = null!;

    // Standard non-GUI-specific implementation of PSHost

    public override CultureInfo CurrentCulture { get; } = Thread.CurrentThread.CurrentCulture;
    public override CultureInfo CurrentUICulture { get; } = Thread.CurrentThread.CurrentUICulture;
    public override Guid InstanceId { get; } = Guid.NewGuid();

    [field: MaybeNull]
    public override PSObject PrivateData => field ??= new(Ctx);

    public override string Name => GetType().Name;

    [field: MaybeNull]
    public override Version Version =>
        field ??=
            TryGetVersion((AssemblyVersionAttribute a) => a.Version)
         ?? TryGetVersion((AssemblyFileVersionAttribute a) => a.Version)
         ?? TryGetVersion((AssemblyInformationalVersionAttribute a) => a.InformationalVersion)
         ?? new(0, 1, 0, 0);

    public override void NotifyBeginApplication() { }

    public override void NotifyEndApplication() { }

    public override void EnterNestedPrompt() { }

    public override void ExitNestedPrompt() { }

    public override void SetShouldExit(int exitCode) { }

    private Version? TryGetVersion<T>(Func<T, string> getter) where T : Attribute
    {
        var attr = GetType().Assembly.GetCustomAttribute<T>();
        return attr != null && Version.TryParse(getter(attr), out var ver) ? ver : null;
    }

    protected static NonInteractiveException NonInteractive() => new();

    protected static RawOutputException RawNotSupported() => new();

    protected internal abstract class PSBaseHostUI(PSBaseTaskContext ctx) : PSHostUserInterface, IHostUISupportsMultipleChoiceSelection
    {
        protected PSBaseTaskContext Ctx { get; set; } = ctx;

        public sealed override PSHostRawUserInterface RawUI => Ctx.Host.RawUI;

        public abstract Collection<int> PromptForChoice(
            string caption, string message, Collection<ChoiceDescription> choices, IEnumerable<int> defaultChoices);

        public override void Write(ConsoleColor foregroundColor, ConsoleColor backgroundColor, string message) =>
            Write(message);

        public override void WriteLine(string message) =>
            Write($"{message}{Endl}");
    }

    protected internal abstract class PSBaseHostRawUI(PSBaseTaskContext ctx) : PSHostRawUserInterface
    {
        protected PSBaseTaskContext Ctx { get; set; } = ctx;
    }
}