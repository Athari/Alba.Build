using System.Diagnostics;
using JetBrains.Annotations;

namespace Alba.Build.PowerShell;

internal class CollectionDebugView<T>(ICollection<T> collection)
{
    private readonly ICollection<T> _collection = Guard.NotNull(collection);

    [PublicAPI]
    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    public T[] Items => [ .._collection ];
}