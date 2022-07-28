namespace AsmResolver.Symbols.Pdb.Leaves;

/// <summary>
/// Represents a single record in a field list of a TPI or IPI stream.
/// </summary>
public abstract class CodeViewField : CodeViewLeaf
{
    private readonly LazyVariable<Utf8String> _name;

    /// <summary>
    /// Initializes an empty CodeView field leaf.
    /// </summary>
    /// <param name="typeIndex">The type index to assign to the leaf.</param>
    protected CodeViewField(uint typeIndex)
        : base(typeIndex)
    {
        _name = new LazyVariable<Utf8String>(GetName);
    }

    /// <summary>
    /// Gets or sets the name of the field.
    /// </summary>
    public Utf8String Name
    {
        get => _name.Value;
        set => _name.Value = value;
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

    /// <summary>
    /// Obtains the name of the field.
    /// </summary>
    /// <returns>The name.</returns>
    /// <remarks>
    /// This method is called upon initialization of the <see cref="Name"/> property.
    /// </remarks>
    protected virtual Utf8String GetName() => Utf8String.Empty;

    /// <inheritdoc />
    public override string ToString() => Name;
}
