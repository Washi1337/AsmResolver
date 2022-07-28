using System.Collections.Generic;
using System.Threading;

namespace AsmResolver.Symbols.Pdb.Leaves;

/// <summary>
/// Represents a leaf record containing a list of overloaded methods.
/// </summary>
public class MethodList : CodeViewLeaf
{
    private IList<MethodListEntry>? _entries;

    /// <summary>
    /// Initializes an empty method list.
    /// </summary>
    /// <param name="typeIndex">The type index to assign to the list.</param>
    protected MethodList(uint typeIndex)
        : base(typeIndex)
    {
    }

    /// <summary>
    /// Creates a new empty method list.
    /// </summary>
    public MethodList()
        : base(0)
    {
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
