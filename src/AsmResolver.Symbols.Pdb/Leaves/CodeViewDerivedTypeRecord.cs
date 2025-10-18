namespace AsmResolver.Symbols.Pdb.Leaves;

/// <summary>
/// Provides a base for all custom types that may be derived from a base type.
/// </summary>
public abstract partial class CodeViewDerivedTypeRecord : CodeViewCompositeTypeRecord
{
    private readonly object _lock = new();

    /// <inheritdoc />
    protected CodeViewDerivedTypeRecord(uint typeIndex)
        : base(typeIndex)
    {
    }

    /// <summary>
    /// Gets or sets the base type that this type is deriving from.
    /// </summary>
    [LazyProperty]
    public partial CodeViewTypeRecord? BaseType
    {
        get;
        set;
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
