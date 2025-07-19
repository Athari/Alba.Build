using System.Management.Automation;
using System.Management.Automation.Language;
using System.Management.Automation.Runspaces;
using JetBrains.Annotations;

namespace Alba.Build.PowerShell;

internal static class Exts
{
    // String

    public static bool IsNullOrEmpty(this string? @this) =>
        string.IsNullOrEmpty(@this);

    public static string? NullIfEmpty(this string? @this) =>
        @this.IsNullOrEmpty() ? null : @this;

    // Enumerable

    public static TResult Aggregate<T, TResult>(this IEnumerable<T> @this, TResult seed, Action<TResult, T> action)
    {
        foreach (var item in @this)
            action(seed, item);
        return seed;
    }

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

    // Type

    /// <remarks><see cref="Type.IsAssignableFrom"/> is "reverse is" and as such is confusing.</remarks>
    public static bool IsAssignableTo(this Type @this, Type type) => type.IsAssignableFrom(@this);

    /// <summary>Equivalent to "is" keyword.</summary>
    public static bool Is(this Type @this, Type type) => @this.IsAssignableTo(type);

    /// <summary>Equivalent to "is" keyword.</summary>
    public static bool Is<T>(this Type @this) => @this.IsAssignableTo(typeof(T));

    // PS AST

    public static IEnumerable<T> FindAll<T>(this Ast @this, Func<T, bool> predicate, bool searchNestedScriptBlocks = false)
        where T : Ast =>
        @this.FindAll(a => a is T t && predicate(t), searchNestedScriptBlocks).Cast<T>();

    public static IEnumerable<T> FindAll<T>(this Ast @this, bool searchNestedScriptBlocks = false)
        where T : Ast =>
        @this.FindAll(a => a is T, searchNestedScriptBlocks).Cast<T>();

    public static string GetName(this ParameterAst @this) =>
        @this.Name.VariablePath.ToString();

    public static string GetName(this CmdletAttribute @this) =>
        $"{@this.VerbName}-{@this.NounName}";

    // Runspace

    public static void SetApartmentState(this Runspace @this, ApartmentState state)
    {
      #if WINDOWS_POWERSHELL
        @this.GetType().GetProperty(nameof(ApartmentState))!.GetSetMethod().Invoke(@this, [ state ]);
      #else
        @this.ApartmentState = ApartmentState.STA;
      #endif
    }
}