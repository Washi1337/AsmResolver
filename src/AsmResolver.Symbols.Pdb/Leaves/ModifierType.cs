namespace AsmResolver.Symbols.Pdb.Leaves;

/// <summary>
/// Represents a type that is annotated with extra modifiers.
/// </summary>
public class ModifierType : CodeViewType
{
    private readonly LazyVariable<CodeViewType> _baseType;

    /// <summary>
    /// Initializes a new empty modifier type.
    /// </summary>
    /// <param name="typeIndex">The type index to assign to the modifier type.</param>
    protected ModifierType(uint typeIndex)
        : base(typeIndex)
    {
        _baseType = new LazyVariable<CodeViewType>(GetBaseType);
    }

    /// <summary>
    /// Creates a new modified type.
    /// </summary>
    /// <param name="type">The type to be modified.</param>
    /// <param name="attributes">The attributes describing the shape of the pointer.</param>
    public ModifierType(CodeViewType type, ModifierAttributes attributes)
        : base(0)
    {
        _baseType = new LazyVariable<CodeViewType>(type);
        Attributes = attributes;
    }

    /// <inheritdoc />
    public override CodeViewLeafKind LeafKind => CodeViewLeafKind.Modifier;

    /// <summary>
    /// Gets or sets the type that is annotated.
    /// </summary>
    public CodeViewType BaseType
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
    /// Obtains the base type of the modifier type.
    /// </summary>
    /// <returns>The base type.</returns>
    /// <remarks>
    /// This method is called upon initialization of the <see cref="BaseType"/> property.
    /// </remarks>
    protected virtual CodeViewType? GetBaseType() => null;

    /// <inheritdoc />
    public override string ToString() => $"{BaseType} {Attributes}";
}
