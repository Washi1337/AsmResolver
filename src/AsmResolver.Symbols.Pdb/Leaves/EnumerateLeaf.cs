namespace AsmResolver.Symbols.Pdb.Leaves;

public class EnumerateLeaf : CodeViewField
{
    private readonly LazyVariable<Utf8String> _name;
    private readonly LazyVariable<object> _value;

    protected EnumerateLeaf(uint typeIndex)
        : base(typeIndex)
    {
        _name = new LazyVariable<Utf8String>(GetName);
        _value = new LazyVariable<object>(GetValue);
    }

    public EnumerateLeaf(Utf8String name, object value, CodeViewFieldAttributes attributes)
        : base(0)
    {
        _name = new LazyVariable<Utf8String>(name);
        _value = new LazyVariable<object>(value);
        Attributes = attributes;
    }

    /// <inheritdoc />
    public override CodeViewLeafKind LeafKind => CodeViewLeafKind.Enumerate;

    /// <summary>
    /// Gets or sets the name of the enumerate field.
    /// </summary>
    public Utf8String Name
    {
        get => _name.Value;
        set => _name.Value = value;
    }

    /// <summary>
    /// Gets or sets the constant value assigned to the field.
    /// </summary>
    public object Value
    {
        get => _value.Value;
        set => _value.Value = value;
    }

    public CodeViewFieldAttributes Attributes
    {
        get;
        set;
    }

    protected virtual Utf8String GetName() => Utf8String.Empty;

    protected virtual object? GetValue() => null;

    /// <inheritdoc />
    public override string ToString() => $"{Name} = {Value}";
}
