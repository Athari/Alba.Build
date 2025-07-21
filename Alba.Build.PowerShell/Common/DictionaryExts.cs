using System.Collections;

namespace Alba.Build.PowerShell;

public static class DictionaryExts
{
    public static IDictionary<TKey, TValue> AsTyped<TKey, TValue>(this IDictionary @this)
        where TKey : notnull =>
        new TypedMap<TKey, TValue>(@this);
}