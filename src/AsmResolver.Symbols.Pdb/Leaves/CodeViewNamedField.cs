namespace AsmResolver.Symbols.Pdb.Leaves;

/// <summary>
/// Represents a single record in a field list that is assigned a name.
/// </summary>
public abstract partial class CodeViewNamedField : CodeViewField
{
    private readonly object _lock = new();

    /// <summary>
    /// Initializes an empty CodeView field leaf.
    /// </summary>
    /// <param name="typeIndex">The type index to assign to the leaf.</param>
    protected CodeViewNamedField(uint typeIndex)
        : base(typeIndex)
    {
    }

    /// <summary>
    /// Gets or sets the name of the field.
    /// </summary>
    [LazyProperty]
    public partial Utf8String Name
    {
        get;
        set;
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
