namespace AsmResolver.Symbols.Pdb.Leaves;

/// <summary>
/// Provides a base for all custom types that may be derived from a base type.
/// </summary>
public abstract class CodeViewDerivedTypeRecord : CodeViewCompositeTypeRecord
{
    private readonly LazyVariable<CodeViewTypeRecord?> _baseType;

    /// <inheritdoc />
    protected CodeViewDerivedTypeRecord(uint typeIndex)
        : base(typeIndex)
    {
        _baseType = new LazyVariable<CodeViewTypeRecord?>(GetBaseType);
    }

    /// <summary>
    /// Gets or sets the base type that this type is deriving from.
    /// </summary>
    public CodeViewTypeRecord? BaseType
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
    protected virtual CodeViewTypeRecord? GetBaseType() => null;

    /// <inheritdoc />
    public override string ToString() => Name;
}
