﻿//<auto-generated/>
#nullable enable
#pragma warning disable

namespace System.Ling;

internal static class EnumerableExts
{
  #if !NETSTANDARD2_1_OR_GREATER && !NETCOREAPP2_0_OR_GREATER && !NET5_0_OR_GREATER

    public static HashSet<TSource> ToHashSet<TSource>(this IEnumerable<TSource> source) =>
        source.ToHashSet(comparer: null);

    public static HashSet<TSource> ToHashSet<TSource>(this IEnumerable<TSource> source, IEqualityComparer<TSource>? comparer)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));
        return new(source, comparer);
    }

  #endif

  #if !NET6_0_OR_GREATER

    public static TSource FirstOrDefault<TSource>(this IEnumerable<TSource> source, TSource defaultValue)
    {
        TSource? first = source.TryGetFirst(out bool found);
        return found ? first! : defaultValue;
    }

    public static TSource FirstOrDefault<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate, TSource defaultValue)
    {
        TSource? first = source.TryGetFirst(predicate, out bool found);
        return found ? first! : defaultValue;
    }

    private static TSource? TryGetFirst<TSource>(this IEnumerable<TSource> source, out bool found)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));

        //if (source is IPartition<TSource> partition)
        //    return partition.TryGetFirst(out found);
        if (source is IList<TSource> list) {
            if (list.Count > 0) {
                found = true;
                return list[0];
            }
        }
        else {
            using IEnumerator<TSource> e = source.GetEnumerator();
            if (e.MoveNext()) {
                found = true;
                return e.Current;
            }
        }
        found = false;
        return default;
    }

    private static TSource? TryGetFirst<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate, out bool found)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (predicate == null) throw new ArgumentNullException(nameof(predicate));

        foreach (TSource element in source) {
            if (predicate(element)) {
                found = true;
                return element;
            }
        }
        found = false;
        return default;
    }

  #endif
}