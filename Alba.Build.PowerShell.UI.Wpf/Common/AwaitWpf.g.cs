using System.Windows.Threading;

namespace Alba.Build.PowerShell.UI.Wpf.Common;

public static partial class AwaitWpf
{
    public static WpfDispatcherAwaiter Send                 { get; } = new(DispatcherPriority.Send           , alwaysYield: false);
    public static WpfDispatcherAwaiter SendYield            { get; } = new(DispatcherPriority.Send           , alwaysYield: true);
    public static WpfDispatcherAwaiter Normal               { get; } = new(DispatcherPriority.Normal         , alwaysYield: false);
    public static WpfDispatcherAwaiter NormalYield          { get; } = new(DispatcherPriority.Normal         , alwaysYield: true);
    public static WpfDispatcherAwaiter DataBind             { get; } = new(DispatcherPriority.DataBind       , alwaysYield: false);
    public static WpfDispatcherAwaiter DataBindYield        { get; } = new(DispatcherPriority.DataBind       , alwaysYield: true);
    public static WpfDispatcherAwaiter Render               { get; } = new(DispatcherPriority.Render         , alwaysYield: false);
    public static WpfDispatcherAwaiter RenderYield          { get; } = new(DispatcherPriority.Render         , alwaysYield: true);
    public static WpfDispatcherAwaiter Loaded               { get; } = new(DispatcherPriority.Loaded         , alwaysYield: false);
    public static WpfDispatcherAwaiter LoadedYield          { get; } = new(DispatcherPriority.Loaded         , alwaysYield: true);
    public static WpfDispatcherAwaiter Input                { get; } = new(DispatcherPriority.Input          , alwaysYield: false);
    public static WpfDispatcherAwaiter InputYield           { get; } = new(DispatcherPriority.Input          , alwaysYield: true);
    public static WpfDispatcherAwaiter Background           { get; } = new(DispatcherPriority.Background     , alwaysYield: false);
    public static WpfDispatcherAwaiter BackgroundYield      { get; } = new(DispatcherPriority.Background     , alwaysYield: true);
    public static WpfDispatcherAwaiter ContextIdle          { get; } = new(DispatcherPriority.ContextIdle    , alwaysYield: false);
    public static WpfDispatcherAwaiter ContextIdleYield     { get; } = new(DispatcherPriority.ContextIdle    , alwaysYield: true);
    public static WpfDispatcherAwaiter ApplicationIdle      { get; } = new(DispatcherPriority.ApplicationIdle, alwaysYield: false);
    public static WpfDispatcherAwaiter ApplicationIdleYield { get; } = new(DispatcherPriority.ApplicationIdle, alwaysYield: true);
    public static WpfDispatcherAwaiter SystemIdle           { get; } = new(DispatcherPriority.SystemIdle     , alwaysYield: false);
    public static WpfDispatcherAwaiter SystemIdleYield      { get; } = new(DispatcherPriority.SystemIdle     , alwaysYield: true);
}