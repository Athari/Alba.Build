using System.Collections;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Alba.Build.PowerShell;

[SuppressMessage("ReSharper", "ParameterTypeCanBeEnumerable.Local", Justification = "Overloads for exact property types")]
[SuppressMessage("Style", "IDE0305:Simplify collection initialization", Justification = "Don't touch LINQ")]
public partial class ScriptParams
{
    private const string IncludeProp = "Include";

    public IReadOnlyDictionary<string, string> Project { get; internal set; } =
        new ReadOnlyDictionary<string, string>(new Dictionary<string, string>());

    private static string? CopyFrom(string? from) =>
        from;

    private static List<string>? CopyFrom(string[]? from) =>
        from?.ToList();

    private static Dictionary<string, string>? CopyFrom(ITaskItem? from) =>
        ItemToDictionary(from);

    private static List<Dictionary<string, string>>? CopyFrom(ITaskItem[]? from) =>
        from?.Select(ItemToDictionary).WhereNotNull().ToList();

    private static string[]? CopyFrom(List<string>? from) =>
        from?.ToArray();

    private static ITaskItem? CopyFrom(Hashtable from) =>
        DictionaryToItem(from);

    private static ITaskItem[] CopyFrom(List<Hashtable> from) =>
        from.Select(DictionaryToItem).WhereNotNull().ToArray();

    private static void Update(Dictionary<string, string>? from, ITaskItem? to)
    {
        if (to == null || !(from?.Count > 0))
            return;

        var existingKeys = to.MetadataNames.Cast<string>().ToHashSet();
        foreach (var (k, v) in from) {
            if (from.Comparer.Equals(k, IncludeProp) || to.GetMetadata(k) == v)
                continue;
            if (!string.IsNullOrEmpty(v))
                to.SetMetadata(k, v);
            else if (existingKeys.Contains(k))
                to.RemoveMetadata(k);
        }
        if (!string.IsNullOrEmpty(from[IncludeProp]))
            to.ItemSpec = from[IncludeProp];
    }

    private static void Update(List<Dictionary<string, string>>? from, ITaskItem[]? to)
    {
        if (to == null || from == null)
            return;

        foreach (var dic in from) {
            var include = dic[IncludeProp];
            if (to.FirstOrDefault(i => i.ItemSpec == include) is not { } existingItem)
                continue;
            Update(dic, existingItem);
        }
    }

    [return: NotNullIfNotNull(nameof(item))]
    public static Dictionary<string, string>? ItemToDictionary(ITaskItem? item)
    {
        if (item == null)
            return null;

        var dic = new Dictionary<string, string> {
            [IncludeProp] = item.ItemSpec,
        };
        foreach (string name in item.MetadataNames)
            dic[name] = item.GetMetadata(name);
        return dic;
    }

    public static ITaskItem? DictionaryToItem(Hashtable? dic)
    {
        if (!(dic?.Count > 0) || !dic.ContainsKey(IncludeProp))
            return null;

        var include = dic[IncludeProp];
        if (include == null)
            return null;

        dic.Remove(IncludeProp);
        return new TaskItem(include.ToString(), dic);
    }
}