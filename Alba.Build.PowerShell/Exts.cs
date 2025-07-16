#if ANCIENT
using System.Diagnostics.CodeAnalysis;
#endif
using System.Diagnostics;
using System.Management.Automation.Language;
using JetBrains.Annotations;

namespace Alba.Build.PowerShell;

internal static class Exts
{
    [Conditional("DEBUG")]
    public static void LaunchDebugger()
    {
        if (!Debugger.IsAttached)
            Debugger.Launch();
    }

    public static bool IsNullOrEmpty(this string? @this) =>
        string.IsNullOrEmpty(@this);

    public static string? NullIfEmpty(this string? @this) =>
        @this.IsNullOrEmpty() ? null : @this;

    public static Dictionary<TKey, T> ToDictionarySafe<T, TKey>(this IEnumerable<T> @this,
        Func<T, TKey> keySelector, IEqualityComparer<TKey>? comparer)
        where TKey : notnull
    {
        var dic = new Dictionary<TKey, T>(comparer ?? EqualityComparer<TKey>.Default);
        foreach (var item in @this)
            dic[keySelector(item)] = item;
        return dic;
    }

    public static Dictionary<string, T> ToDictionarySafeOIC<T>(this IEnumerable<T> @this,
        Func<T, string> keySelector) =>
        @this.ToDictionarySafe(keySelector, StringComparer.OrdinalIgnoreCase);


    public static Dictionary<TKey, TValue> ToDictionarySafe<T, TKey, TValue>(this IEnumerable<T> @this,
        Func<T, TKey> keySelector, Func<T, TValue> valueSelector, IEqualityComparer<TKey>? comparer)
        where TKey : notnull
    {
        var dic = new Dictionary<TKey, TValue>(comparer ?? EqualityComparer<TKey>.Default);
        foreach (var item in @this)
            dic[keySelector(item)] = valueSelector(item);
        return dic;
    }

    public static Dictionary<string, TValue> ToDictionarySafeOIC<T, TValue>(this IEnumerable<T> @this,
        Func<T, string> keySelector, Func<T, TValue> valueSelector) =>
        @this.ToDictionarySafe(keySelector, valueSelector, StringComparer.OrdinalIgnoreCase);

    public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> @this)
    {
        return @this.Where(i => i != null)!;
    }

    public static void AddRange<T>(this ICollection<T> @this, [InstantHandle] IEnumerable<T> items)
    {
        foreach (T item in items)
            @this.Add(item);
    }

    /// <remarks><see cref="Type.IsAssignableFrom"/> is "reverse is" and as such is confusing.</remarks>
    public static bool IsAssignableTo(this Type @this, Type type) => type.IsAssignableFrom(@this);

    /// <summary>Equivalent to "is" keyword.</summary>
    public static bool Is(this Type @this, Type type) => @this.IsAssignableTo(type);

    /// <summary>Equivalent to "is" keyword.</summary>
    public static bool Is<T>(this Type @this) => @this.IsAssignableTo(typeof(T));

    public static IEnumerable<T> FindAll<T>(this Ast @this, Func<T, bool> predicate, bool searchNestedScriptBlocks = false)
        where T : Ast =>
        @this.FindAll(a => a is T t && predicate(t), searchNestedScriptBlocks).Cast<T>();

    public static IEnumerable<T> FindAll<T>(this Ast @this, bool searchNestedScriptBlocks = false)
        where T : Ast =>
        @this.FindAll(a => a is T, searchNestedScriptBlocks).Cast<T>();

    public static string GetName(this ParameterAst @this) =>
        @this.Name.VariablePath.ToString();

  #if ANCIENT
    [SuppressMessage("ReSharper", "ReturnTypeCanBeNotNullable", Justification = "No, it can't")]
    public static TValue? GetValueOrDefault<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> @this, TKey key) =>
        @this.GetValueOrDefault(key, default!);

    public static TValue GetValueOrDefault<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> @this, TKey key, TValue defaultValue) =>
        @this.TryGetValue(key, out TValue? value) ? value : defaultValue;

    public static HashSet<T> ToHashSet<T>(this IEnumerable<T> @this) => [ ..@this ];

    public static void Deconstruct<TKey, TValue>(this KeyValuePair<TKey, TValue> @this, out TKey key, out TValue value)
    {
        key = @this.Key;
        value = @this.Value;
    }
  #endif
}