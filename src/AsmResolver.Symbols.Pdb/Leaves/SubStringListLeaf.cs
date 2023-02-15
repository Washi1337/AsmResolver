using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace AsmResolver.Symbols.Pdb.Leaves;

/// <summary>
/// Represents a list of sub strings associated to a String ID entry.
/// </summary>
public class SubStringListLeaf : CodeViewLeaf, IIpiLeaf
{
    private IList<StringIdentifier>? _entries;

    /// <summary>
    /// Initializes an empty sub-string list.
    /// </summary>
    /// <param name="typeIndex">The type index associated to the list.</param>
    protected SubStringListLeaf(uint typeIndex)
        : base(typeIndex)
    {
    }

    /// <summary>
    /// Creates a new empty list of sub-strings.
    /// </summary>
    public SubStringListLeaf()
        : this(Enumerable.Empty<StringIdentifier>())
    {
    }

    /// <summary>
    /// Creates a new list of sub-strings.
    /// </summary>
    /// <param name="elements">The strings to add.</param>
    public SubStringListLeaf(params StringIdentifier[] elements)
        : this(elements.AsEnumerable())
    {
    }

    /// <summary>
    /// Creates a new list of sub-strings.
    /// </summary>
    /// <param name="elements">The strings to add.</param>
    public SubStringListLeaf(IEnumerable<StringIdentifier> elements)
        : base(0)
    {
        _entries = new List<StringIdentifier>(elements);
    }

    /// <inheritdoc />
    public override CodeViewLeafKind LeafKind => CodeViewLeafKind.SubstrList;

    /// <summary>
    /// Gets a collection of entries stored in the list.
    /// </summary>
    public IList<StringIdentifier> Entries
    {
        get
        {
            if (_entries is null)
                Interlocked.CompareExchange(ref _entries, GetEntries(), null);
            return _entries;
        }
    }

    /// <summary>
    /// Obtains the sub strings stored in the list.
    /// </summary>
    /// <returns>The sub strings.</returns>
    /// <remarks>
    /// This method is called upon initialization of the <see cref="Entries"/> property.
    /// </remarks>
    protected virtual IList<StringIdentifier> GetEntries() => new List<StringIdentifier>();
}
