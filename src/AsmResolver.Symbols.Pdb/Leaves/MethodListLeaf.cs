using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace AsmResolver.Symbols.Pdb.Leaves;

/// <summary>
/// Represents a leaf record containing a list of overloaded methods.
/// </summary>
public class MethodListLeaf : CodeViewLeaf
{
    private IList<MethodListEntry>? _entries;

    /// <summary>
    /// Initializes an empty method list.
    /// </summary>
    /// <param name="typeIndex">The type index to assign to the list.</param>
    protected MethodListLeaf(uint typeIndex)
        : base(typeIndex)
    {
    }

    /// <summary>
    /// Creates a new empty method list.
    /// </summary>
    public MethodListLeaf()
        : base(0)
    {
    }

    /// <summary>
    /// Creates a new method list.
    /// </summary>
    /// <param name="entries">The methods to include.</param>
    public MethodListLeaf(params MethodListEntry[] entries)
        : base(0)
    {
        _entries = entries.ToList();
    }

    /// <inheritdoc />
    public override CodeViewLeafKind LeafKind => CodeViewLeafKind.MethodList;

    /// <summary>
    /// Gets a collection of methods stored in the list.
    /// </summary>
    public IList<MethodListEntry> Entries
    {
        get
        {
            if (_entries is null)
                Interlocked.CompareExchange(ref _entries, GetEntries(), null);
            return _entries;
        }
    }

    /// <summary>
    /// Obtains the methods stored in the list.
    /// </summary>
    /// <returns>The methods</returns>
    /// <remarks>
    /// This method is called upon initialization of the <see cref="Entries"/> property.
    /// </remarks>
    protected virtual IList<MethodListEntry> GetEntries() => new List<MethodListEntry>();
}
