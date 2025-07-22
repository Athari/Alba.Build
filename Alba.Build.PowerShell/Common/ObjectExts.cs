using JetBrains.Annotations;

namespace Alba.Build.PowerShell.Common;

[PublicAPI]
public static class ObjectExts
{
    [Pure]
    public static bool Compare<T>(T x, T y, Func<bool> compare)
    {
        if (ReferenceEquals(x, y))
            return true;
        if (x is null || y is null || x.GetType() != y.GetType())
            return false;
        return compare();
    }

    [Pure]
    public static bool Equals<T>(this T @this, object? obj, Func<T, bool> equals)
    {
        if (obj is null) return false;
        if (ReferenceEquals(obj, @this)) return true;
        return obj.GetType() == @this?.GetType() && equals((T)obj);
    }

    [Pure]
    public static bool EqualsAny<T>(this T @this, params IEnumerable<T> values) => values.Any(v => @this.EqualsValue(v));

    [Pure]
    public static bool EqualsValue<T>(this T @this, T value) => EqualityComparer<T>.Default.Equals(@this, value);

    [Pure]
    public static string GetTypeFullName(this object? @this) => @this == null ? "null" : @this.GetType().FullName!;

    public static T As<T>(this T @this, out T var) => var = @this;

    [Pure]
    public static T To<T>(this object @this) => (T)@this;

    [Pure]
    public static bool IsAnyType(this object? @this, params IEnumerable<Type> types) =>
        @this == null || types.Any(@this.GetType().IsAssignableTo);

    public static T With<T>(this T @this, Action<T> action)
    {
        action(@this);
        return @this;
    }
}