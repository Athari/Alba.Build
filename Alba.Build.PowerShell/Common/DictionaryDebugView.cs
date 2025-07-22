using System.Diagnostics;
using JetBrains.Annotations;

namespace Alba.Build.PowerShell.Common;

[PublicAPI]
internal class DictionaryDebugView<TKey, TValue>(IDictionary<TKey, TValue> dictionary)
{
    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    public KeyValuePair<TKey, TValue>[] Items
    {
        get
        {
            var array = new KeyValuePair<TKey, TValue>[dictionary.Count];
            dictionary.CopyTo(array, 0);
            return array;
        }
    }
}