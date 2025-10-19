using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace AsmResolver.SourceGenerators;

internal readonly record struct EquatableArray<T>(ImmutableArray<T> Array) : IReadOnlyList<T>
{
    public int Count => Array.Length;

    public T this[int index] => Array[index];

    public bool Equals(EquatableArray<T>? other) => other is { Array: var elements } && Array.SequenceEqual(elements);

    public override int GetHashCode()
    {
        int hash = 0;
        foreach (var value in Array)
        {
            hash = hash * 37 + (value is null ? 0 : value.GetHashCode());
        }

        return hash;
    }

    ImmutableArray<T>.Enumerator GetEnumerator() => Array.GetEnumerator();

    IEnumerator<T> IEnumerable<T>.GetEnumerator() => ((IReadOnlyList<T>) Array).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable) Array).GetEnumerator();
}
