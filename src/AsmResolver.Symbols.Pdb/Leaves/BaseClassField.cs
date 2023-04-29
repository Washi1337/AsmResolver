namespace AsmResolver.Symbols.Pdb.Leaves;

/// <summary>
/// Represents a reference to a base class object in a structure.
/// </summary>
public class BaseClassField : CodeViewField
{
    private readonly LazyVariable<BaseClassField, CodeViewTypeRecord?> _type;

    /// <summary>
    /// Initializes an empty base class.
    /// </summary>
    /// <param name="typeIndex">The type index to assign to the base class field.</param>
    protected BaseClassField(uint typeIndex)
        : base(typeIndex)
    {
        _type = new LazyVariable<BaseClassField, CodeViewTypeRecord?>(x => x.GetBaseType());
    }

    /// <summary>
    /// Creates a new base class field.
    /// </summary>
    /// <param name="type">The base type to reference.</param>
    public BaseClassField(CodeViewTypeRecord type)
        : base(0)
    {
        _type = new LazyVariable<BaseClassField, CodeViewTypeRecord?>(type);
    }

    /// <inheritdoc />
    public override CodeViewLeafKind LeafKind => CodeViewLeafKind.BClass;

    /// <summary>
    /// Gets or sets the base type that this base class is referencing.
    /// </summary>
    public CodeViewTypeRecord? Type
    {
        get => _type.GetValue(this);
        set => _type.SetValue(value);
    }

    /// <summary>
    /// Gets or sets the offset of the base within the class.
    /// </summary>
    public ulong Offset
    {
        get;
        set;
    }

    /// <summary>
    /// Obtains the base type that the class is referencing.
    /// </summary>
    /// <returns>The base type.</returns>
    /// <remarks>
    /// This method is called upon initialization of the <see cref="Type"/> property.
    /// </remarks>
    protected virtual CodeViewTypeRecord? GetBaseType() => null;

    /// <inheritdoc />
    public override string ToString() => Type?.ToString() ?? "<<<?>>>";
}
