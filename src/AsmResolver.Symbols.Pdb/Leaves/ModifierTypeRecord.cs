namespace AsmResolver.Symbols.Pdb.Leaves;

/// <summary>
/// Represents a type that is annotated with extra modifiers.
/// </summary>
public class ModifierTypeRecord : CodeViewTypeRecord
{
    private readonly LazyVariable<CodeViewTypeRecord> _baseType;

    /// <summary>
    /// Initializes a new empty modifier type.
    /// </summary>
    /// <param name="typeIndex">The type index to assign to the modifier type.</param>
    protected ModifierTypeRecord(uint typeIndex)
        : base(typeIndex)
    {
        _baseType = new LazyVariable<CodeViewTypeRecord>(GetBaseType);
    }

    /// <summary>
    /// Creates a new modified type.
    /// </summary>
    /// <param name="type">The type to be modified.</param>
    /// <param name="attributes">The attributes describing the shape of the pointer.</param>
    public ModifierTypeRecord(CodeViewTypeRecord type, ModifierAttributes attributes)
        : base(0)
    {
        _baseType = new LazyVariable<CodeViewTypeRecord>(type);
        Attributes = attributes;
    }

    /// <inheritdoc />
    public override CodeViewLeafKind LeafKind => CodeViewLeafKind.Modifier;

    /// <summary>
    /// Gets or sets the type that is annotated.
    /// </summary>
    public CodeViewTypeRecord BaseType
    {
        get => _baseType.Value;
        set => _baseType.Value = value;
    }

    /// <summary>
    /// Gets or sets the annotations that were added to the type.
    /// </summary>
    public ModifierAttributes Attributes
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets a value indicating whether the type is marked as const.
    /// </summary>
    public bool IsConst
    {
        get => (Attributes & ModifierAttributes.Const) != 0;
        set => Attributes = (Attributes & ~ModifierAttributes.Const)
                            | (value ? ModifierAttributes.Const : 0);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the type is marked as volatile.
    /// </summary>
    public bool IsVolatile
    {
        get => (Attributes & ModifierAttributes.Volatile) != 0;
        set => Attributes = (Attributes & ~ModifierAttributes.Volatile)
                            | (value ? ModifierAttributes.Volatile : 0);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the type is marked as unaligned.
    /// </summary>
    public bool IsUnaligned
    {
        get => (Attributes & ModifierAttributes.Unaligned) != 0;
        set => Attributes = (Attributes & ~ModifierAttributes.Unaligned)
                            | (value ? ModifierAttributes.Unaligned : 0);
    }

    /// <summary>
    /// Obtains the base type of the modifier type.
    /// </summary>
    /// <returns>The base type.</returns>
    /// <remarks>
    /// This method is called upon initialization of the <see cref="BaseType"/> property.
    /// </remarks>
    protected virtual CodeViewTypeRecord? GetBaseType() => null;

    /// <inheritdoc />
    public override string ToString() => $"{BaseType} {Attributes}";
}
