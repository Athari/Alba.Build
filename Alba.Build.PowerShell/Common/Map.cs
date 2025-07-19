using System.Collections;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Alba.Build.PowerShell;

/// <summary>
/// Map is to <see cref="Dictionary{TKey,TValue}"/> what <see cref="Collection{T}"/> is to <see cref="List{T}"/>.
/// Allows to override methods of Dictionary.
/// Some members of non-generic interface are only supported if the underlying dictionary supports <see cref="IDictionary"/> interface.
/// </summary>
[Serializable, ComVisible(false)]
[DebuggerDisplay("Count = {Count}"), DebuggerTypeProxy(typeof(DictionaryDebugView<,>))]
internal class Map<TKey, TValue>(IDictionary<TKey, TValue> dictionary)
    : IDictionary<TKey, TValue>, IDictionary, IReadOnlyDictionary<TKey, TValue>
    where TKey : notnull
{
    private readonly IDictionary<TKey, TValue> _dictionary = Guard.NotNull(dictionary);

    public Map() : this(new Dictionary<TKey, TValue>()) { }

    private IDictionary? IdSafe => _dictionary as IDictionary;
    private IDictionary Id => IdSafe ?? throw MissingNonGeneric();

    public int Count => _dictionary.Count;

    public bool IsReadOnly => _dictionary.IsReadOnly;

    bool IDictionary.IsFixedSize => IdSafe?.IsFixedSize ?? false;
    bool ICollection.IsSynchronized => IdSafe?.IsSynchronized ?? false;
    object ICollection.SyncRoot => IdSafe?.SyncRoot ?? this;

    public TValue this[TKey key]
    {
        get => TryGetItem(key, out TValue value) ? value : throw KeyNotFound($"{key}");
        set => IfNotReadOnly(() => SetItem(key, value));
    }

    object? IDictionary.this[object key]
    {
        get => TryGetItem(Guard.NotNullObject<TKey>(key), out TValue v) ? v : throw KeyNotFound($"{key}");
        set => IfNotReadOnly(() => SetItem(Guard.NotNullObject<TKey>(key), Guard.NullableOrNotNullObject<TValue>(value)));
    }

    public void Add(TKey key, TValue value) =>
        IfNotReadOnly(() => AddItem(key, value));

    public bool ContainsKey(TKey key) =>
        TryGetItem(key, out _);

    private bool Contains(TKey key, TValue value) =>
        TryGetItem(key, out TValue v) && EqualityComparer<TValue>.Default.Equals(v, value);

    public bool TryGetValue(TKey key, out TValue value) =>
        TryGetItem(key, out value);

    public bool Remove(TKey key) => IfNotReadOnly(() =>
        RemoveItem(key));

    public void Clear() =>
        IfNotReadOnly(ClearItems);

    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int index) => _dictionary.CopyTo(array, index);

    void IDictionary.Add(object key, object? value) =>
        Add(Guard.NotNullObject<TKey>(key), Guard.NullableOrNotNullObject<TValue>(value));

    bool IDictionary.Contains(object key) =>
        Guard.TryNotNullObject<TKey>(key, out var k) && ContainsKey(k);

    void IDictionary.Remove(object key) =>
        IfNotReadOnly(() => Guard.IfNotNullObject<TKey>(key, k => RemoveItem(k)));

    void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item) =>
        Add(item.Key, item.Value);

    bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item) =>
        Contains(item.Key, item.Value);

    bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item) =>
        IfNotReadOnly(() => Contains(item.Key, item.Value) && RemoveItem(item.Key));

    void ICollection.CopyTo(Array array, int index) =>
        Id.CopyTo(array, index);

    protected virtual bool TryGetItem(TKey key, out TValue value) => _dictionary.TryGetValue(key, out value!);

    protected virtual void SetItem(TKey key, TValue value) => _dictionary[key] = value;

    protected virtual void AddItem(TKey key, TValue value) => _dictionary.Add(key, value);

    protected virtual bool RemoveItem(TKey key) => _dictionary.Remove(key);

    protected virtual void ClearItems() => _dictionary.Clear();

    public ICollection<TKey> Keys => _dictionary.Keys;
    IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => Keys;
    ICollection IDictionary.Keys => Id.Keys;

    public ICollection<TValue> Values => _dictionary.Values;
    IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => Values;
    ICollection IDictionary.Values => Id.Values;

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => _dictionary.GetEnumerator();
    IDictionaryEnumerator IDictionary.GetEnumerator() => Id.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => Id.GetEnumerator();

    private static NotSupportedException ReadOnly() =>
        new("Dictionary is read-only.");

    private static NotSupportedException MissingNonGeneric() =>
        new("Underlying dictionary does not implement non-generic IDictionary interface.");

    private static KeyNotFoundException KeyNotFound(string key) =>
        new($"Key '{key}' not found.");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool GuardNotReadOnly()
    {
        if (IsReadOnly)
            throw ReadOnly();
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void IfNotReadOnly(Action action)
    {
        GuardNotReadOnly();
        action();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private T IfNotReadOnly<T>(Func<T> fun)
    {
        GuardNotReadOnly();
        return fun();
    }

    [MemberNotNull(nameof(IdSafe))]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool GuardHasNonGeneric()
    {
        if (IdSafe == null)
            throw MissingNonGeneric();
        return true;
    }
}