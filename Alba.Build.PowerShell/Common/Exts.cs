using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Management.Automation.Language;
using System.Management.Automation.Runspaces;
using JetBrains.Annotations;

namespace Alba.Build.PowerShell.Common;

internal static class Exts
{
    // String

    [ContractAnnotation("null => true")]
    public static bool IsNullOrEmpty(this string? @this) =>
        string.IsNullOrEmpty(@this);

    [ContractAnnotation("null => null; notnull => canbenull")]
    public static string? NullIfEmpty(this string? @this) =>
        @this.IsNullOrEmpty() ? null : @this;

    public static string Q(this string? @this, char q) => $"{q}{@this}{q}";
    public static string Q(this string? @this, char ql, char qr) => $"{ql}{@this}{qr}";
    public static string Qg(this string? @this) => $"`{@this}`"; // grave
    public static string Qs1(this string? @this) => $"'{@this}'"; // straight inner
    public static string Qs2(this string? @this) => $"\"{@this}\""; // straight outer
    public static string Qt1(this string? @this) => $"‘{@this}’"; // typographic inner
    public static string Qt2(this string? @this) => $"“{@this}”"; // typographic outer
    public static string Qtg1(this string? @this) => $"‚{@this}‘"; // typographic german inner
    public static string Qtg2(this string? @this) => $"„{@this}“"; // typographic german outer
    public static string Qa1(this string? @this) => $"‹{@this}›"; // angle inner (french)
    public static string Qa2(this string? @this) => $"«{@this}»"; // angle outer (french)
    public static string Qai1(this string? @this) => $"›{@this}‹"; // angle inverted inner
    public static string Qai2(this string? @this) => $"»{@this}«"; // angle inverted outer
    public static string Qc1(this string? @this) => $"「{@this}」"; // corner inner (chinese primary)
    public static string Qc2(this string? @this) => $"『{@this}』"; // corner outer (chinese secondary)
    public static string Qch1(this string? @this) => $"｢{@this}｣"; // corner half-width inner
    public static string Qch2(this string? @this) => $"『{@this}』"; // corner half-width outer
    public static string Qk1(this string? @this) => $"〈{@this}〉"; // chinese book / korean inner
    public static string Qk2(this string? @this) => $"《{@this}》"; // chinese book / korean outer

    // Enumerable

    public static TResult Aggregate<T, TResult>(this IEnumerable<T> @this, TResult seed, Action<TResult, T> action)
    {
        foreach (var item in @this)
            action(seed, item);
        return seed;
    }

    public static Collection<T> ToCollection<T>(this IList<T> @this) =>
        new(@this);

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