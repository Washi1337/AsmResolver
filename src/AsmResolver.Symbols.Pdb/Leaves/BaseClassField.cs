namespace AsmResolver.Symbols.Pdb.Leaves;

/// <summary>
/// Represents a reference to a base class object in a structure.
/// </summary>
public partial class BaseClassField : CodeViewField
{
    private readonly object _lock = new();

    /// <summary>
    /// Initializes an empty base class.
    /// </summary>
    /// <param name="typeIndex">The type index to assign to the base class field.</param>
    protected BaseClassField(uint typeIndex)
        : base(typeIndex)
    {
    }

    /// <summary>
    /// Creates a new base class field.
    /// </summary>
    /// <param name="baseType">The base type to reference.</param>
    public BaseClassField(CodeViewTypeRecord baseType)
        : base(0)
    {
        BaseType = baseType;
    }

    /// <inheritdoc />
    public override CodeViewLeafKind LeafKind => CodeViewLeafKind.BClass;

    /// <summary>
    /// Gets or sets the base type that this base class is referencing.
    /// </summary>
    [LazyProperty]
    public partial CodeViewTypeRecord? BaseType
    {
        get;
        set;
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
    /// This method is called upon initialization of the <see cref="BaseType"/> property.
    /// </remarks>
    protected virtual CodeViewTypeRecord? GetBaseType() => null;

    /// <inheritdoc />
    public override string ToString() => BaseType?.ToString() ?? "<<<?>>>";
}
