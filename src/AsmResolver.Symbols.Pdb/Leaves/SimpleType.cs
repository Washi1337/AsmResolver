namespace AsmResolver.Symbols.Pdb.Leaves;

/// <summary>
/// Represents a simple type referenced by a simple type index.
/// </summary>
public class SimpleType : CodeViewType
{
    /// <summary>
    /// Constructs a new simple type based on the provided type index.
    /// </summary>
    /// <param name="typeIndex">The type index.</param>
    public SimpleType(uint typeIndex)
        : base(typeIndex)
    {
    }

    /// <summary>
    /// Constructs a new simple type with the provided type kind.
    /// </summary>
    /// <param name="kind">The type kind.</param>
    public SimpleType(SimpleTypeKind kind)
        : base((uint) kind)
    {
    }

    /// <summary>
    /// Constructs a new simple type with the provided type kind and mode.
    /// </summary>
    /// <param name="kind">The type kind.</param>
    /// <param name="mode">The mode indicating the pointer specifiers added to the type.</param>
    public SimpleType(SimpleTypeKind kind, SimpleTypeMode mode)
        : base((uint) kind | ((uint) mode << 8))
    {
    }

    /// <inheritdoc />
    public override CodeViewLeafKind LeafKind => CodeViewLeafKind.SimpleType;

    /// <summary>
    /// Gets the kind of the simple type.
    /// </summary>
    public SimpleTypeKind Kind => (SimpleTypeKind) (TypeIndex & 0b1111_1111);

    /// <summary>
    /// Gets the mode describing the pointer specifiers that are added to the simple type.
    /// </summary>
    public SimpleTypeMode Mode => (SimpleTypeMode) ((TypeIndex >> 8) & 0b1111);

    /// <summary>
    /// Gets a value indicating whether the type is a pointer or not.
    /// </summary>
    public bool IsPointer => Mode != SimpleTypeMode.Direct;

    /// <inheritdoc />
    public override string ToString() => $"{Kind} ({Mode})";
}
