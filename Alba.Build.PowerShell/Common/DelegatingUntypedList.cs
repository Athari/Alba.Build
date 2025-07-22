using System.Collections;
using System.Diagnostics;

namespace Alba.Build.PowerShell;

[DebuggerDisplay("Count = {Count}"), DebuggerTypeProxy(typeof(CollectionDebugView<>))]
internal class DelegatingUntypedList<T>(ICollection collection, CollectionOptions options = CollectionOptions.None)
    : UntypedList<T>(collection, options)
{
    public Func<int, T>? Getter { get; init; }
    public Action<int, T>? Setter { get; init; }
    public Action<int, T>? Inserter { get; init; }
    public Action<int>? Remover { get; init; }
    public Action? Clearer { get; init; }

    protected override T GetItem(int index)
    {
        return Getter != null
            ? Getter(index)
            : base.GetItem(index);
    }

    protected override void SetItem(int index, T item)
    {
        if (Setter != null)
            Setter(index, item);
        else
            base.SetItem(index, item);
    }

    protected override void InsertItem(int index, T item)
    {
        if (Inserter != null)
            Inserter(index, item);
        else
            base.InsertItem(index, item);
    }

    protected override void RemoveItem(int index)
    {
        if (Remover != null)
            Remover(index);
        else
            base.RemoveItem(index);
    }

    protected override void ClearItems()
    {
        if (Clearer != null)
            Clearer();
        else
            base.ClearItems();
    }
}