namespace AsmResolver.Symbols.Pdb.Leaves;

/// <summary>
/// Represents a single record in a field list of a TPI or IPI stream.
/// </summary>
public abstract class CodeViewField : CodeViewLeaf
{
    /// <summary>
    /// Initializes an empty CodeView field leaf.
    /// </summary>
    /// <param name="typeIndex">The type index to assign to the leaf.</param>
    protected CodeViewField(uint typeIndex)
        : base(typeIndex)
    {
    }
}
