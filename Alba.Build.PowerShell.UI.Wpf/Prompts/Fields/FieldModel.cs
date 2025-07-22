using System.Collections.ObjectModel;
using System.Management.Automation.Host;
using Alba.Build.PowerShell.Common;
using Alba.Build.PowerShell.UI.Wpf.Common;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Alba.Build.PowerShell.UI.Wpf.Prompts.Fields;

internal abstract partial class FieldModel(Type type) : ModelBase
{
    public Collection<Attribute> Attributes { get; } = [ ];

    public Type Type { get; private set; } = type;

    public string Name { get; private set; } = "";

    public string? Label { get; private set; }

    [ObservableProperty]
    public partial object? Value { get; set; }

    public object? DefaultValue { get; private set; }

    public string? HelpMessage { get; private set; }

    public bool IsMandatory { get; private set; }

    public static FieldModel Create(FieldDescription d)
    {
        var type = (d.ParameterTypeFullName, d.ParameterAssemblyFullName) switch {
            ({ } typeName, { } assemblyName) =>
                Type.GetType($"{typeName}, {assemblyName}") ??
                throw new ArgumentException($"Could not resolve parameter type {typeName.Qt2()} from assembly {assemblyName.Qt2()}."),
            ({ } typeName, null) =>
                Type.GetType(typeName) ??
                throw new ArgumentException($"Could not resolve parameter type {typeName.Qt2()}."),
            (_, _) => typeof(string),
        };
        FieldModel m = type switch {
            { IsEnum: true } => new EnumFieldModel(type),
            _ => new StringFieldModel(type),
        };
        m.Type = type;
        m.Name = d.Name ?? "";
        m.Label = d.Label.NullIfEmpty();
        m.Value = m.DefaultValue = d.DefaultValue;
        m.HelpMessage = d.HelpMessage.NullIfEmpty();
        m.IsMandatory = d.IsMandatory;
        return m;
    }
}

internal partial class StringFieldModel(Type type) : FieldModel(type) { }

internal partial class EnumFieldModel(Type type) : FieldModel(type) { }