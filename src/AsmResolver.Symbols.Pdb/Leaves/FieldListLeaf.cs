using System.Collections.Generic;
using System.Threading;

namespace AsmResolver.Symbols.Pdb.Leaves;

/// <summary>
/// Represents a leaf containing a list of fields.
/// </summary>
public class FieldListLeaf : CodeViewLeaf, ITpiLeaf
{
    private IList<CodeViewField>? _fields;

    /// <summary>
    /// Initializes an empty field list.
    /// </summary>
    /// <param name="typeIndex">The type index to assign to the list.</param>
    protected FieldListLeaf(uint typeIndex)
        : base(typeIndex)
    {
    }

    /// <summary>
    /// Creates a new empty field list.
    /// </summary>
    public FieldListLeaf()
        : base(0)
    {
    }

    /// <inheritdoc />
    public override CodeViewLeafKind LeafKind => CodeViewLeafKind.FieldList;

    /// <summary>
    /// Gets a collection of fields stored in the list.
    /// </summary>
    public IList<CodeViewField> Entries
    {
        get
        {
            if (_fields is null)
                Interlocked.CompareExchange(ref _fields, GetEntries(), null);
            return _fields;
        }
    }

    /// <summary>
    /// Obtains the fields stored in the list.
    /// </summary>
    /// <returns>The fields</returns>
    /// <remarks>
    /// This method is called upon initialization of the <see cref="Entries"/> property.
    /// </remarks>
    protected virtual IList<CodeViewField> GetEntries() => new List<CodeViewField>();
}
