#nullable enable
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Alba.Build.PowerShell.Tasks;
using Microsoft.Build.Framework;

namespace Alba.Build.PowerShell.Tasks;

public partial class ExecPowerShellTask
{
    public string? String1 { get; set; }
    public string? String2 { get; set; }
    public string? String3 { get; set; }
    public string? String4 { get; set; }
    public string[]? Strings1 { get; set; }
    public string[]? Strings2 { get; set; }
    public string[]? Strings3 { get; set; }
    public string[]? Strings4 { get; set; }
    public ITaskItem? Item1 { get; set; }
    public ITaskItem? Item2 { get; set; }
    public ITaskItem? Item3 { get; set; }
    public ITaskItem? Item4 { get; set; }
    public ITaskItem[]? Items1 { get; set; }
    public ITaskItem[]? Items2 { get; set; }
    public ITaskItem[]? Items3 { get; set; }
    public ITaskItem[]? Items4 { get; set; }
    [Output] public string? OutString1 { get; set; }
    [Output] public string? OutString2 { get; set; }
    [Output] public string? OutString3 { get; set; }
    [Output] public string? OutString4 { get; set; }
    [Output] public string[]? OutStrings1 { get; set; }
    [Output] public string[]? OutStrings2 { get; set; }
    [Output] public string[]? OutStrings3 { get; set; }
    [Output] public string[]? OutStrings4 { get; set; }
    [Output] public ITaskItem? OutItem1 { get; set; }
    [Output] public ITaskItem? OutItem2 { get; set; }
    [Output] public ITaskItem? OutItem3 { get; set; }
    [Output] public ITaskItem? OutItem4 { get; set; }
    [Output] public ITaskItem[]? OutItems1 { get; set; }
    [Output] public ITaskItem[]? OutItems2 { get; set; }
    [Output] public ITaskItem[]? OutItems3 { get; set; }
    [Output] public ITaskItem[]? OutItems4 { get; set; }
}