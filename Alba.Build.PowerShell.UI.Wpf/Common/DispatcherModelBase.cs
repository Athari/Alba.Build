using System.Windows.Threading;

namespace Alba.Build.PowerShell.UI.Wpf.Common;

internal class DispatcherModelBase(Dispatcher dispatcher) : ModelBase
{
    public Dispatcher Dispatcher { get; } = dispatcher;
}