namespace AsmResolver.Symbols.Pdb.Leaves;

/// <summary>
/// Provides a base for all custom types that may be derived from a base type.
/// </summary>
public abstract class CodeViewDerivedType : CodeViewCompositeType
{
    private readonly LazyVariable<CodeViewType?> _baseType;

    /// <inheritdoc />
    protected CodeViewDerivedType(uint typeIndex)
        : base(typeIndex)
    {
        _baseType = new LazyVariable<CodeViewType?>(GetBaseType);
    }

    /// <summary>
    /// Gets or sets the base type that this type is deriving from.
    /// </summary>
    public CodeViewType? BaseType
    {
        get => _baseType.Value;
        set => _baseType.Value = value;
    }

    /// <summary>
    /// Obtains the type that the type is derived from.
    /// </summary>
    /// <returns>The type.</returns>
    /// <remarks>
    /// This method is called upon initialization of the <see cref="BaseType"/> property.
    /// </remarks>
    protected virtual CodeViewType? GetBaseType() => null;

    /// <inheritdoc />
    public override string ToString() => Name;
}
