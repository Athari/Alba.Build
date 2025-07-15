namespace Alba.Build.PowerShell;

internal static class Exts
{
    public static Dictionary<TKey, T> ToDictionarySafe<T, TKey>(this IEnumerable<T> @this,
        Func<T, TKey> keySelector, IEqualityComparer<TKey>? comparer)
        where TKey : notnull
    {
        var dic = new Dictionary<TKey, T>(comparer ?? EqualityComparer<TKey>.Default);
        foreach (var item in @this)
            dic[keySelector(item)] = item;
        return dic;
    }

    public static Dictionary<TKey, TValue> ToDictionarySafe<T, TKey, TValue>(this IEnumerable<T> @this,
        Func<T, TKey> keySelector, Func<T, TValue> valueSelector, IEqualityComparer<TKey>? comparer)
        where TKey : notnull
    {
        var dic = new Dictionary<TKey, TValue>(comparer ?? EqualityComparer<TKey>.Default);
        foreach (var item in @this)
            dic[keySelector(item)] = valueSelector(item);
        return dic;
    }

    public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> @this)
    {
        return @this.Where(i => i != null)!;
    }

  #if ANCIENT

    public static HashSet<T> ToHashSet<T>(this IEnumerable<T> @this) => [ ..@this ];

    public static void Deconstruct<TKey, TValue>(this KeyValuePair<TKey, TValue> @this, out TKey key, out TValue value)
    {
        key = @this.Key;
        value = @this.Value;
    }

  #endif
}