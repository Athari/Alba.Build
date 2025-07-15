using System.Collections.ObjectModel;
using System.Management.Automation;

namespace Alba.Build.PowerShell;

internal class PowerShellOutputCollection(PSShell shell, PowerShellHost host) : Collection<PSObject>
{
    private const string FormatClassIdProperty = "ClassId2e4f51ef21dd47e99d3c952918aff9cd";
    private const string FormatEndClassId = "cf522b78d86c486691226b40aa69e95c";

    private readonly List<PSObject> _buffer = [ ];

    protected override void InsertItem(int index, PSObject item)
    {
        base.InsertItem(index, item);
        ProcessNewItem(item);
    }

    protected override void SetItem(int index, PSObject item)
    {
        base.SetItem(index, item);
        ProcessNewItem(item);
    }

    private void ProcessNewItem(PSObject item)
    {
        if (PSObjectToString(item) is { } s)
            host.UI.Write(s);
    }

    private string? PSObjectToString(PSObject o)
    {
        if (o.BaseObject == null)
            return "null";

        if (o.Properties[FormatClassIdProperty]?.Value is not string classId)
            return CallOutString(o);

        _buffer.Add(o);
        if (classId != FormatEndClassId)
            return null;

        var str = CallOutString(_buffer);
        _buffer.Clear();
        return str;
    }

    private string? CallOutString(object o)
    {
        using var ps = shell.CreateNestedPowerShell();
        return ps.AddCommand("Out-String").AddParameter("InputObject", o).Invoke<string>().FirstOrDefault();
    }
}