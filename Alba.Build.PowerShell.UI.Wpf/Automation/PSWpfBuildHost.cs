using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Management.Automation.Runspaces;
using System.Security;
using System.Windows.Threading;
using Alba.Build.PowerShell.Common;
using Alba.Build.PowerShell.UI.Wpf.Common;
using Alba.Build.PowerShell.UI.Wpf.Main;
using Alba.Build.PowerShell.UI.Wpf.Tasks;

namespace Alba.Build.PowerShell.UI.Wpf;

internal class PSWpfBuildHost : PSBuildHost
{
    protected new PSWpfBuildTaskContext Ctx {
        get => (PSWpfBuildTaskContext)base.Ctx;
        private protected set => base.Ctx = value;
    }

    public new PSWpfBuildHostUI UI {
        get => (PSWpfBuildHostUI)_UI;
        private protected set => _UI = value;
    }

    public new PSWpfBuildHostRawUI RawUI {
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

        //var mre = new ManualResetEvent(false);
        //AwaitWpf.UIDispatcher.InvokeAsync(() => {
        //    _ = new Application {
        //        MainWindow = Ctx.Window,
        //        ShutdownMode = ShutdownMode.OnExplicitShutdown,
        //        Resources = new() {
        //            MergedDictionaries = {
        //                new() { Source = new("/PresentationFramework.Luna,Version=4.0.0.0,PublicKeyToken=31bf3856ad364e35;component/themes/Luna.Metallic.xaml", UriKind.Relative) },
        //            },
        //        },
        //    };
        //    mre.Set();
        //    Dispatcher.PushFrame(new());
        //});
        //mre.WaitOne();
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

    private static async Task<Result<T>> CallWithTimeout<T>(Task<Result<T>> task, TimeSpan timeout)
    {
        return await await Task.WhenAny(task, CancelOnTimeout());

        async Task<Result<T>> CancelOnTimeout()
        {
            await Task.Delay(timeout);
            return Result.Cancel<T>();
        }
    }

    private T CallUI<T>(Func<MainModel, Task<Result<T>>> ui) =>
        CallWithTimeout(
                Ctx.Invoke(() => {
                    var task = ui(Ctx.Model);
                    Ctx.Window.ShowFront();
                    return task;
                }),
                Ctx.Task.UITimeoutSpan)
            .GetResultSync().GetValueOrThrow();

    internal class PSWpfBuildHostUI(PSWpfBuildTaskContext ctx) : PSBuildHostUI(ctx)
    {
        protected new PSWpfBuildTaskContext Ctx {
            get => (PSWpfBuildTaskContext)base.Ctx;
            private protected set => base.Ctx = value;
        }

        public override Dictionary<string, PSObject?> Prompt(
            string caption, string message, Collection<FieldDescription> descriptions) =>
            Ctx.Host.CallUI(m =>
                    m.PromptFields(caption, message, descriptions))
                .ToDictionary(p => p.Key, p => p.Value != null ? new PSObject(p.Value) : null);

        public override int PromptForChoice(
            string caption, string message, Collection<ChoiceDescription> choices, int defaultChoice) =>
            Ctx.Host.CallUI(m =>
                m.PromptChoiceSingle(caption, message, choices, defaultChoice));

        public override Collection<int> PromptForChoice(
            string caption, string message, Collection<ChoiceDescription> choices, IEnumerable<int> defaultChoices) =>
            Ctx.Host.CallUI(m =>
                    m.PromptChoiceMultiple(caption, message, choices, [ ..defaultChoices ]))
                .ToCollection();

        public override PSCredential? PromptForCredential(
            string caption, string message, string userName, string targetName) =>
            PromptForCredential(caption, message, userName, targetName, PSCredentialTypes.Default, PSCredentialUIOptions.Default);

        public override PSCredential? PromptForCredential(
            string caption, string message, string userName, string targetName,
            PSCredentialTypes allowedTypes, PSCredentialUIOptions options) =>
            Ctx.Host.CallUI(m =>
                m.PromptCredential(caption, message, userName, targetName, allowedTypes, options));

        public override string? ReadLine() =>
            Ctx.Host.CallUI(m =>
                m.ReadLine(GetDefaultPrompt()));

        public override SecureString? ReadLineAsSecureString() =>
            Ctx.Host.CallUI(m =>
                m.SecureReadLine(GetDefaultPrompt()));

        private string GetDefaultPrompt() =>
            Buffer.Length > 0 ? Buffer.ToString().Trim() : LastMessage ?? "Please enter text";
    }

    internal class PSWpfBuildHostRawUI(PSWpfBuildTaskContext ctx) : PSBuildHostRawUI(ctx)
    {
        protected new PSWpfBuildTaskContext Ctx {
            get => (PSWpfBuildTaskContext)base.Ctx;
            private protected set => base.Ctx = value;
        }

        public override string WindowTitle {
            get => Ctx.Invoke(() => Ctx.Model.Title);
            set => Ctx.Invoke(() => Ctx.Model.Title = value);
        }
    }
}