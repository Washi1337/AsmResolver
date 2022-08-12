using System.Collections.Generic;
using System.Threading;

namespace AsmResolver.Symbols.Pdb.Leaves;

/// <summary>
/// Describes the shape of the virtual function table of a class or structure type.
/// </summary>
public class VTableShapeLeaf : CodeViewLeaf
{
    private IList<VTableShapeEntry>? _entries;

    /// <summary>
    /// Initializes a new empty virtual function table shape.
    /// </summary>
    /// <param name="typeIndex">The type index to assign to the shape.</param>
    protected VTableShapeLeaf(uint typeIndex)
        : base(typeIndex)
    {
    }

    /// <summary>
    /// Creates a new empty virtual function table shape.
    /// </summary>
    public VTableShapeLeaf()
        : base(0)
    {
    }

    /// <summary>
    /// Creates a new virtual function table shape with the provided entries.
    /// </summary>
    public VTableShapeLeaf(params VTableShapeEntry[] entries)
        : base(0)
    {
        _entries = new List<VTableShapeEntry>(entries);
    }

    /// <inheritdoc />
    public override CodeViewLeafKind LeafKind => CodeViewLeafKind.VTShape;

    /// <summary>
    /// Gets the list of entries that defines the shape of the virtual function table.
    /// </summary>
    public IList<VTableShapeEntry> Entries
    {
        get
        {
            if (_entries is null)
                Interlocked.CompareExchange(ref _entries, GetEntries(), null);
            return _entries;
        }
    }

    /// <summary>
    /// Obtains the list of entries stored in the shape.
    /// </summary>
    /// <returns>The entries.</returns>
    /// <remarks>
    /// This method is called upon initialization of the <see cref="Entries"/> property.
    /// </remarks>
    protected virtual IList<VTableShapeEntry> GetEntries() => new List<VTableShapeEntry>();
}
