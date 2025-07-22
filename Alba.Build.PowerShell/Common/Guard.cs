using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace Alba.Build.PowerShell.Common;

internal static class Guard
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T NotNull<T>(T? param,
        [InvokerParameterName, CallerArgumentExpression(nameof(param))] string? paramName = null)
    {
        if (param is { } p)
            return p;
        throw new ArgumentNullException(paramName, $"Argument {nameof(param)} must not be null");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T NullableOrNotNullObject<T>(object? param,
        [InvokerParameterName, CallerArgumentExpression(nameof(param))] string? paramName = null)
    {
        if (default(T) is null && param is null)
            return default!;
        if (param is T p)
            return p;
        throw new ArgumentException($"Argument {nameof(param)} must be of type {typeof(T)} and not null", paramName);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryNullableOrNotNullObject<T>(object? param, out T result)
    {
        if (default(T) is null && param is null) {
            result = default!;
            return true;
        }
        else if (param is T p) {
            result = p;
            return true;
        }
        else {
            result = default!;
            return false;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IfNullableOrNotNullObject<T>(object? param, Action<T> action)
    {
        if (default(T) is null && param is null) {
            action(default!);
            return true;
        }
        else if (param is T p) {
            action(p);
            return true;
        }
        else {
            return false;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T NotNullObject<T>(object? param,
        [InvokerParameterName, CallerArgumentExpression(nameof(param))] string? paramName = null)
    {
        if (param is T p)
            return p;
        throw new ArgumentException($"Argument {nameof(param)} must be of type {typeof(T)}", paramName);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryNotNullObject<T>(object? param, out T result)
    {
        if (param is T p) {
            result = p;
            return true;
        }
        else {
            result = default!;
            return false;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IfNotNullObject<T>(object? param, Action<T> action)
    {
        if (param is T p) {
            action(p);
            return true;
        }
        else {
            return false;
        }
    }
}