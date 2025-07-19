using Alba.Build.PowerShell.UI.Wpf.Common;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Alba.Build.PowerShell.UI.Wpf.Main;

public partial class MainModel : ModelBase
{
    [ObservableProperty]
    public partial string Title { get; private set; } = "Title";

    public MainModel()
    {
    }

    public async Task Loaded() { }

    public async Task Closing() { }
}