using System.Reflection;
using System.Windows;
using Alba.Build.PowerShell.Common;
using Alba.Build.PowerShell.UI.Wpf.Tasks;

namespace Alba.Build.PowerShell.UI.Wpf.Main;

internal partial class MainWindow : Window
{
    public static Uri ThemeUri { get; set; } = GetThemeUri(ExecPowerShellWpfTask.DefaultUITheme, ExecPowerShellWpfTask.DefaultUIVersion);

    public MainWindow(PSWpfBuildTaskContext ctx)
    {
        ThemeUri = GetThemeUri(ctx.Task.UITheme, ctx.Task.UIThemeVersion);
        InitializeComponent();
        DataContext = ctx.Model;
    }

    public MainWindow ShowFront()
    {
        if (!IsVisible)
            Show();
        if (WindowState == WindowState.Minimized)
            WindowState = WindowState.Normal;
        Activate();
        Topmost = true;
        Topmost = false;
        Focus();
        return this;
    }

    private static Uri GetThemeUri(string theme, Version version) =>
        new AssemblyName { Name = $"PresentationFramework.{theme.Split('.')[0]}", Version = version }
            .WithMicrosoftPublicKeyToken().ToPackUri($"/Themes/{theme}.xaml");
}