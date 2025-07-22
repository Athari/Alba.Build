using System.Collections;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Alba.Build.PowerShell;

[DebuggerDisplay("Count = {Count}"), DebuggerTypeProxy(typeof(CollectionDebugView<>))]
internal class UntypedList<T>(ICollection collection, CollectionOptions options = CollectionOptions.Default)
    : IList
{
    private readonly ICollection _collection = Guard.NotNull(collection);
    private readonly CollectionOptions _options = options | CollectionOptions.None;

    public int Count => _collection.Count;
    public bool IsReadOnly => (_options & CollectionOptions.ReadOnly) != 0;

    bool IList.IsFixedSize => false;
    bool ICollection.IsSynchronized => _collection.IsSynchronized;
    object ICollection.SyncRoot => _collection.SyncRoot;

    public T this[int index] {
        get => GetItem(index);
        set => SetItem(index, value);
    }

    object? IList.this[int index] {
        get => this[index];
        set => this[index] = Guard.NullableOrNotNullObject<T>(value);
    }

    // public

    public bool Contains(T item)
    {
        var it = GetEnumerator();
        try {
            while (it.MoveNext()) {
                if (EqualityComparer<T>.Default.Equals(it.Current, item))
                    return true;
            }
        }
        finally {
            it.Dispose();
        }
        return false;
    }

    public int IndexOf(T item)
    {
        int index = 0;
        var it = GetEnumerator();
        try {
            while (it.MoveNext()) {
                if (EqualityComparer<T>.Default.Equals(it.Current, item))
                    return index;
                index++;
            }
        }
        finally {
            it.Dispose();
        }
        return -1;
    }

    public void Add(T item) =>
        Insert(Count, item);

    public void Insert(int index, T item)
    {
        GuardNotReadOnly();
        if (index < 0 || index > _collection.Count)
            throw new ArgumentOutOfRangeException(nameof(index));
        InsertItem(index, item);
    }

    public bool Remove(T item)
    {
        GuardNotReadOnly();
        int index = IndexOf(item);
        if (index < 0)
            return false;
        RemoveItem(index);
        return true;
    }

    public void RemoveAt(int index)
    {
        GuardNotReadOnly();
        if (index < 0 || index >= _collection.Count)
            throw new ArgumentOutOfRangeException(nameof(index));
        RemoveItem(index);
    }

    public void Clear() =>
        IfNotReadOnly(ClearItems);

    public void CopyTo(T[] array, int index) =>
        _collection.CopyTo(array, index);

    public IEnumerator<T> GetEnumerator()
    {
        var it = _collection.GetEnumerator();
        try {
            while (it.MoveNext())
                yield return (T)it.Current!;
        }
        finally {
            (it as IDisposable)?.Dispose();
        }
    }

    // IList

    bool IList.Contains(object? value) =>
        Guard.TryNullableOrNotNullObject<T>(value, out var v) && Contains(v);

    int IList.IndexOf(object? value) =>
        Guard.TryNullableOrNotNullObject<T>(value, out var v) ? IndexOf(v) : -1;

    int IList.Add(object? value)
    {
        var index = Count;
        Add(Guard.NullableOrNotNullObject<T>(value));
        return index;
    }

    void IList.Insert(int index, object? value) =>
        Insert(index, Guard.NullableOrNotNullObject<T>(value));

    void IList.Remove(object? value) =>
        Guard.IfNullableOrNotNullObject<T>(value, v => Remove(v));

    // ICollection

    void ICollection.CopyTo(Array array, int index) =>
        _collection.CopyTo(array, index);

    // IEnumerable

    IEnumerator IEnumerable.GetEnumerator() =>
        _collection.GetEnumerator();

    // virtual

    protected virtual T GetItem(int index) =>
        throw NotSupported("List does not support retrieval by index.");

    protected virtual void SetItem(int index, T item) =>
        throw NotSupported("List does not support assignment by index.");

    protected virtual void InsertItem(int index, T item) =>
        throw NotSupported("List does not support insertion.");

    protected virtual void RemoveItem(int index) =>
        throw NotSupported("List does not support removal.");

    protected virtual void ClearItems() =>
        throw NotSupported("List does not support clearing.");

    // utility

    private static NotSupportedException NotSupported(string message) =>
        new(message);

    private static NotSupportedException ReadOnly() =>
        NotSupported("List is read-only.");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void GuardNotReadOnly()
    {
        if (IsReadOnly)
            throw ReadOnly();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void IfNotReadOnly(Action action)
    {
        GuardNotReadOnly();
        action();
    }
}