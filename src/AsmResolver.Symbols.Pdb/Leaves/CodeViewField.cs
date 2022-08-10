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

    /// <summary>
    /// Gets or sets the attributes associated to the field.
    /// </summary>
    public CodeViewFieldAttributes Attributes
    {
        get;
        set;
    }

    /// <summary>
    /// Gets a value indicating whether the field is a newly introduced virtual function.
    /// </summary>
    public bool IsIntroducingVirtual =>
        (Attributes & CodeViewFieldAttributes.IntroducingVirtual) == CodeViewFieldAttributes.IntroducingVirtual
        || (Attributes & CodeViewFieldAttributes.PureIntroducingVirtual) == CodeViewFieldAttributes.PureIntroducingVirtual;
}
