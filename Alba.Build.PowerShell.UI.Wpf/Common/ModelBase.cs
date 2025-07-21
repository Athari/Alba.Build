using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Alba.Build.PowerShell.UI.Wpf.Common;

internal class ModelBase : ObservableObject
{
    protected Disposable WithFlag(Action<bool> set)
    {
        set(true);
        return new(() => set(false));
    }

    protected Disposable WithFlag(Action<bool> set, DispatcherPriority priority)
    {
        set(true);
        return new(() => Dispose().NoAwait());

        async Task Dispose()
        {
            await priority;
            set(false);
        }
    }
}