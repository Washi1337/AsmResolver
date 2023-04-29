namespace AsmResolver.Symbols.Pdb.Leaves;

/// <summary>
/// Represents a single enumerate field leaf in a field list.
/// </summary>
public class EnumerateField : CodeViewNamedField
{
    private readonly LazyVariable<EnumerateField, object> _value;

    /// <summary>
    /// Initializes an empty enumerate field leaf.
    /// </summary>
    /// <param name="typeIndex">The type index to assign to the enumerate field.</param>
    protected EnumerateField(uint typeIndex)
        : base(typeIndex)
    {
        _value = new LazyVariable<EnumerateField, object>(x => x.GetValue());
    }

    /// <summary>
    /// Creates a new enumerate field leaf.
    /// </summary>
    /// <param name="name">The name of the field.</param>
    /// <param name="value">The value assigned to the field.</param>
    /// <param name="attributes">The attributes associated to the field.</param>
    public EnumerateField(Utf8String name, object value, CodeViewFieldAttributes attributes)
        : base(0)
    {
        Name = name;
        _value = new LazyVariable<EnumerateField, object>(value);
        Attributes = attributes;
    }

    /// <inheritdoc />
    public override CodeViewLeafKind LeafKind => CodeViewLeafKind.Enumerate;

    /// <summary>
    /// Gets or sets the constant value assigned to the field.
    /// </summary>
    public object Value
    {
        get => _value.GetValue(this);
        set => _value.SetValue(value);
    }

    /// <summary>
    /// Obtains the value assigned to the field.
    /// </summary>
    /// <returns>The value.</returns>
    /// <remarks>
    /// This method is called upon initialization of the <see cref="Value"/> property.
    /// </remarks>
    protected virtual object? GetValue() => null;

    /// <inheritdoc />
    public override string ToString() => $"{Name} = {Value}";
}
