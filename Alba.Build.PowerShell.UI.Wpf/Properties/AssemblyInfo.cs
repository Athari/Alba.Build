using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Markup;
using static Alba.Build.PowerShell.UI.Wpf.Properties.S;

[assembly: ThemeInfo(ResourceDictionaryLocation.None, ResourceDictionaryLocation.SourceAssembly)]
[assembly: XmlnsPrefix(UrnGui, PrefixGui)]
[assembly: XmlnsDefinition(UrnGui, $"{NsGui}")]
[assembly: XmlnsDefinition(UrnGui, $"{NsGui}.Common")]
[assembly: XmlnsDefinition(UrnGui, $"{NsGui}.Main")]

namespace Alba.Build.PowerShell.UI.Wpf.Properties;

[SuppressMessage("ReSharper", "CheckNamespace")]
file static class S
{
    public const string UrnGui = "urn:alba:powershell:wpf";
    public const string NsGui = "Alba.Build.PowerShell.UI.Wpf";
    public const string PrefixGui = "my";
}