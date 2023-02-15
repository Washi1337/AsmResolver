namespace AsmResolver.Symbols.Pdb.Leaves;

/// <summary>
/// Represents a single leaf record in a TPI or IPI stream.
/// </summary>
public interface ICodeViewLeaf
{
    /// <summary>
    /// Gets the type kind this record encodes.
    /// </summary>
    CodeViewLeafKind LeafKind
    {
        get;
    }

    /// <summary>
    /// Gets the type index the type is associated to.
    /// </summary>
    uint TypeIndex
    {
        get;
    }
}
