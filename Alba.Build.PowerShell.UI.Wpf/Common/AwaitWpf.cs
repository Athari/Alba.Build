using System.Diagnostics.CodeAnalysis;
using System.Windows.Threading;
using JetBrains.Annotations;

namespace Alba.Build.PowerShell.UI.Wpf.Common;

[PublicAPI]
[SuppressMessage("ReSharper", "MethodOverloadWithOptionalParameter", Justification = "GetAwaiter method must have no parameters to be recognized")]
public static partial class AwaitWpf
{
    private static Thread? _UIThread;
    private static Dispatcher? _UIDispatcher;

    public static Dispatcher UIDispatcher
    {
        get => _UIDispatcher ?? throw new InvalidOperationException($"{nameof(UIDispatcher)} not initialized yet.");
        set
        {
            if (_UIDispatcher != null)
                throw new InvalidOperationException($"{nameof(UIDispatcher)} already initialized.");
            _UIDispatcher = value;
        }
    }

    public static void CreateUIDispatcher()
    {
        if (_UIDispatcher != null)
            return;

        var ready = new ManualResetEvent(false);
        _UIThread = new(() => {
            UIDispatcher = Dispatcher.CurrentDispatcher;
            ready.Set();
            Dispatcher.Run();
        }) {
            Name = "UIThread",
        };
        _UIThread.SetApartmentState(ApartmentState.STA);
        _UIThread.Start();
        ready.WaitOne();
    }

    public static void StopUIDispatcher()
    {
        if (_UIDispatcher == null)
            return;

        _UIDispatcher.InvokeShutdown();
        _UIDispatcher = null;
        _UIThread = null;
    }

    public static WpfDispatcherAwaiter GetAwaiter(this DispatcherPriority @this) =>
        new(@this);

    public static WpfDispatcherAwaiter GetAwaiter(this DispatcherPriority @this, bool alwaysYield = false) =>
        new(@this, alwaysYield);

    [PublicAPI]
    public readonly struct WpfDispatcherAwaiter(DispatcherPriority priority, bool alwaysYield = false)
        : IAwaiter<WpfDispatcherAwaiter>
    {
        public bool IsCompleted => !alwaysYield && UIDispatcher.CheckAccess();
        public void OnCompleted(Action continuation) => UIDispatcher.InvokeAsync(continuation, priority);
        public void UnsafeOnCompleted(Action continuation) => OnCompleted(continuation);
        public WpfDispatcherAwaiter GetAwaiter() => this;
        public void GetResult() { }
    }
}