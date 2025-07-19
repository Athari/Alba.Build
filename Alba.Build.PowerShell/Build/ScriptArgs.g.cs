#nullable enable
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Alba.Build.PowerShell.Tasks;
using Microsoft.Build.Framework;

namespace Alba.Build.PowerShell;

public partial class ScriptArgs
{
    public string? String1 { get; set; }
    public string? String2 { get; set; }
    public string? String3 { get; set; }
    public string? String4 { get; set; }
    public List<string>? Strings1 { get; set; }
    public List<string>? Strings2 { get; set; }
    public List<string>? Strings3 { get; set; }
    public List<string>? Strings4 { get; set; }
    public Dictionary<string, string>? Item1 { get; set; }
    public Dictionary<string, string>? Item2 { get; set; }
    public Dictionary<string, string>? Item3 { get; set; }
    public Dictionary<string, string>? Item4 { get; set; }
    public List<Dictionary<string, string>>? Items1 { get; set; }
    public List<Dictionary<string, string>>? Items2 { get; set; }
    public List<Dictionary<string, string>>? Items3 { get; set; }
    public List<Dictionary<string, string>>? Items4 { get; set; }

    public string OutString1 { get; set; } = "";
    public string OutString2 { get; set; } = "";
    public string OutString3 { get; set; } = "";
    public string OutString4 { get; set; } = "";
    public List<string> OutStrings1 { get; set; } = [ ];
    public List<string> OutStrings2 { get; set; } = [ ];
    public List<string> OutStrings3 { get; set; } = [ ];
    public List<string> OutStrings4 { get; set; } = [ ];
    public Hashtable OutItem1 { get; set; } = [ ];
    public Hashtable OutItem2 { get; set; } = [ ];
    public Hashtable OutItem3 { get; set; } = [ ];
    public Hashtable OutItem4 { get; set; } = [ ];
    public List<Hashtable> OutItems1 { get; set; } = [ ];
    public List<Hashtable> OutItems2 { get; set; } = [ ];
    public List<Hashtable> OutItems3 { get; set; } = [ ];
    public List<Hashtable> OutItems4 { get; set; } = [ ];

    public void CopyFromTask(ExecPowerShellTask task)
    {
        String1 = CopyFrom(task.String1);
        String2 = CopyFrom(task.String2);
        String3 = CopyFrom(task.String3);
        String4 = CopyFrom(task.String4);
        Strings1 = CopyFrom(task.Strings1);
        Strings2 = CopyFrom(task.Strings2);
        Strings3 = CopyFrom(task.Strings3);
        Strings4 = CopyFrom(task.Strings4);
        Item1 = CopyFrom(task.Item1);
        Item2 = CopyFrom(task.Item2);
        Item3 = CopyFrom(task.Item3);
        Item4 = CopyFrom(task.Item4);
        Items1 = CopyFrom(task.Items1);
        Items2 = CopyFrom(task.Items2);
        Items3 = CopyFrom(task.Items3);
        Items4 = CopyFrom(task.Items4);
    }

    public void CopyToTask(ExecPowerShellTask task)
    {
        task.OutString1 = CopyFrom(OutString1);
        task.OutString2 = CopyFrom(OutString2);
        task.OutString3 = CopyFrom(OutString3);
        task.OutString4 = CopyFrom(OutString4);
        task.OutStrings1 = CopyFrom(OutStrings1);
        task.OutStrings2 = CopyFrom(OutStrings2);
        task.OutStrings3 = CopyFrom(OutStrings3);
        task.OutStrings4 = CopyFrom(OutStrings4);
        task.OutItem1 = CopyFrom(OutItem1);
        task.OutItem2 = CopyFrom(OutItem2);
        task.OutItem3 = CopyFrom(OutItem3);
        task.OutItem4 = CopyFrom(OutItem4);
        task.OutItems1 = CopyFrom(OutItems1);
        task.OutItems2 = CopyFrom(OutItems2);
        task.OutItems3 = CopyFrom(OutItems3);
        task.OutItems4 = CopyFrom(OutItems4);
        Update(Item1, task.Item1);
        Update(Item2, task.Item2);
        Update(Item3, task.Item3);
        Update(Item4, task.Item4);
        Update(Items1, task.Items1);
        Update(Items2, task.Items2);
        Update(Items3, task.Items3);
        Update(Items4, task.Items4);
    }

    public bool TryGetInputValue(string name, out object? value)
    {
        (bool Success, object? Value) ret = name switch {
            "Args" => (true, this),
            nameof(String1) => (true, String1),
            nameof(String2) => (true, String2),
            nameof(String3) => (true, String3),
            nameof(String4) => (true, String4),
            nameof(Strings1) => (true, Strings1),
            nameof(Strings2) => (true, Strings2),
            nameof(Strings3) => (true, Strings3),
            nameof(Strings4) => (true, Strings4),
            nameof(Item1) => (true, Item1),
            nameof(Item2) => (true, Item2),
            nameof(Item3) => (true, Item3),
            nameof(Item4) => (true, Item4),
            nameof(Items1) => (true, Items1),
            nameof(Items2) => (true, Items2),
            nameof(Items3) => (true, Items3),
            nameof(Items4) => (true, Items4),
            _ => (false, null),
        };
        value = ret.Value;
        return ret.Success;
    }
}