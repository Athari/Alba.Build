using System.Collections;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Alba.Build.PowerShell;

[Serializable]
[DebuggerDisplay("Count = {Count}"), DebuggerTypeProxy(typeof(CollectionDebugView<>))]
internal class TypedCollection<T>(ICollection collection /*, CollectionOptions options = CollectionOptions.Default*/)
    : ICollection<T>, IReadOnlyCollection<T>, ICollection
{
    private readonly ICollection _collection = Guard.NotNull(collection);
    //private readonly CollectionOptions _options = options | CollectionOptions.None;

    public int Count => _collection.Count;

    public bool IsReadOnly => true;

    bool ICollection.IsSynchronized => _collection.IsSynchronized;
    object ICollection.SyncRoot => _collection.SyncRoot;

    // public

    public bool Contains(T item) =>
        _collection.Cast<T>().Contains(item);

    public virtual void Add(T item) => throw ReadOnly();

    public virtual void Clear() => throw ReadOnly();

    public virtual bool Remove(T item) => throw ReadOnly();

    public IEnumerator<T> GetEnumerator() => _collection.Cast<T>().GetEnumerator();

    public void CopyTo(T[] array, int arrayIndex) => _collection.CopyTo(array, arrayIndex);

    // ICollection

    void ICollection.CopyTo(Array array, int index) => _collection.CopyTo(array, index);

    // IEnumerable

    IEnumerator IEnumerable.GetEnumerator() => _collection.GetEnumerator();

    // utility

    private static NotSupportedException ReadOnly() =>
        new("Dictionary is read-only.");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool GuardNotReadOnly()
    {
        if (IsReadOnly)
            throw ReadOnly();
        return true;
    }
}