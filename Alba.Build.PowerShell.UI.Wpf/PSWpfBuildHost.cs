using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Management.Automation.Runspaces;
using System.Security;
using System.Windows.Threading;
using Alba.Build.PowerShell.UI.Wpf.Common;
using Alba.Build.PowerShell.UI.Wpf.Tasks;

namespace Alba.Build.PowerShell.UI.Wpf;

internal class PSWpfBuildHost : PSBuildHost
{
    protected new PSWpfBuildTaskContext Ctx
    {
        get => (PSWpfBuildTaskContext)base.Ctx;
        private protected set => base.Ctx = value;
    }

    public new PSWpfBuildHostUI UI
    {
        get => (PSWpfBuildHostUI)_UI;
        private protected set => _UI = value;
    }

    public new PSWpfBuildHostRawUI RawUI
    {
        get => (PSWpfBuildHostRawUI)base.RawUI;
        private protected set => base.RawUI = value;
    }

    private protected PSWpfBuildHost() { }

    public PSWpfBuildHost(PSShell shell, ExecPowerShellWpfTask task)
    {
        Ctx = new(shell, this, task);
        UI = new(Ctx);
        RawUI = new(Ctx);
    }

    protected override void InitSession(InitialSessionState session)
    {
        base.InitSession(session);
        session.ThreadOptions = PSThreadOptions.ReuseThread;
        AwaitWpf.CreateUIDispatcher();
        AwaitWpf.UIDispatcher.UnhandledException += UIDispatcher_OnUnhandledException;
    }

    protected override void InitRunspace(Runspace runspace)
    {
        base.InitRunspace(runspace);
        runspace.SetApartmentState(ApartmentState.STA);
        runspace.ThreadOptions = PSThreadOptions.ReuseThread;
    }

    private void UIDispatcher_OnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        UI.LogException(LogLevel.Error, e.Exception, withStackTrace: null,
            "Unhandled dispatcher exception.", ErrorCat.Build, ErrorCode.InteractiveError);
    }

    internal class PSWpfBuildHostUI(PSWpfBuildTaskContext ctx) : PSBuildHostUI(ctx)
    {
        protected new PSWpfBuildTaskContext Ctx
        {
            get => (PSWpfBuildTaskContext)base.Ctx;
            private protected set => base.Ctx = value;
        }

        public override Dictionary<string, PSObject> Prompt(string caption, string message, Collection<FieldDescription> descriptions)
        {
            throw NonInteractive();
        }

        public override int PromptForChoice(string caption, string message, Collection<ChoiceDescription> choices, int defaultChoice)
        {
            throw NonInteractive();
        }

        public override Collection<int> PromptForChoice(string caption, string message, Collection<ChoiceDescription> choices, IEnumerable<int> defaultChoices)
        {
            throw NonInteractive();
        }

        public override PSCredential PromptForCredential(string caption, string message, string userName, string targetName)
        {
            return PromptForCredential(caption, message, userName, targetName,
                PSCredentialTypes.Default, PSCredentialUIOptions.Default);
        }

        public override PSCredential PromptForCredential(string caption, string message, string userName, string targetName, PSCredentialTypes allowedCredentialTypes, PSCredentialUIOptions options)
        {
            throw NonInteractive();
        }

        public override string ReadLine()
        {
            throw NonInteractive();
        }

        public override SecureString ReadLineAsSecureString()
        {
            return ReadLine().Aggregate(new SecureString(), (s, c) => s.AppendChar(c));
        }
    }

    internal class PSWpfBuildHostRawUI(PSWpfBuildTaskContext ctx) : PSBuildHostRawUI(ctx)
    {
        protected new PSWpfBuildTaskContext Ctx
        {
            get => (PSWpfBuildTaskContext)base.Ctx;
            private protected set => base.Ctx = value;
        }

        public override string WindowTitle
        {
            get => Ctx.Invoke(() => Ctx.MainWindow.Title ?? "");
            set => Ctx.Invoke(() => Ctx.MainWindow.Title = value);
        }
    }
}