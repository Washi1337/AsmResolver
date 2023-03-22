namespace AsmResolver.Symbols.Pdb.Leaves;

/// <summary>
/// Represents a single type record in a TPI or IPI stream.
/// </summary>
public abstract class CodeViewTypeRecord : CodeViewLeaf, ITpiLeaf
{
    /// <summary>
    /// Initializes an empty CodeView type record.
    /// </summary>
    /// <param name="typeIndex">The type index to assign to the leaf.</param>
    protected CodeViewTypeRecord(uint typeIndex)
        : base(typeIndex)
    {
    }
}
