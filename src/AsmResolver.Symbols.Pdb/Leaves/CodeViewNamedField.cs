namespace AsmResolver.Symbols.Pdb.Leaves;

/// <summary>
/// Represents a single record in a field list that is assigned a name.
/// </summary>
public abstract class CodeViewNamedField : CodeViewField
{
    private readonly LazyVariable<CodeViewNamedField, Utf8String> _name;

    /// <summary>
    /// Initializes an empty CodeView field leaf.
    /// </summary>
    /// <param name="typeIndex">The type index to assign to the leaf.</param>
    protected CodeViewNamedField(uint typeIndex)
        : base(typeIndex)
    {
        _name = new LazyVariable<CodeViewNamedField, Utf8String>(x => x.GetName());
    }

    /// <summary>
    /// Gets or sets the name of the field.
    /// </summary>
    public Utf8String Name
    {
        get => _name.GetValue(this);
        set => _name.SetValue(value);
    }

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
