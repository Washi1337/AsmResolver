namespace AsmResolver.Symbols.Pdb.Leaves;

/// <summary>
/// Represents a leaf containing build information for a file in a PDB image.
/// </summary>
public class BuildInfoLeaf : SubStringListLeaf
{
    /// <summary>
    /// Initializes an empty build information leaf.
    /// </summary>
    /// <param name="typeIndex">The type index associated to the leaf.</param>
    protected BuildInfoLeaf(uint typeIndex)
        : base(typeIndex)
    {
    }

    /// <inheritdoc />
    public override CodeViewLeafKind LeafKind => CodeViewLeafKind.BuildInfo;
}
