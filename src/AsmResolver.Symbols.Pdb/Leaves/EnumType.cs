namespace AsmResolver.Symbols.Pdb.Leaves;

/// <summary>
/// Represents an enum type.
/// </summary>
public class EnumType : CodeViewDerivedType
{
    /// <summary>
    /// Initializes a new empty enum type.
    /// </summary>
    /// <param name="typeIndex">The type index to assign to the enum type.</param>
    protected EnumType(uint typeIndex)
        : base(typeIndex)
    {
    }

    /// <summary>
    /// Creates a new enum type.
    /// </summary>
    /// <param name="name">The name of the enum.</param>
    /// <param name="underlyingType">The underlying type of all members in the enum.</param>
    /// <param name="attributes">The structural attributes assigned to the enum.</param>
    public EnumType(Utf8String name, CodeViewType underlyingType, StructureAttributes attributes)
        : base(0)
    {
        Name = name;
        BaseType = underlyingType;
        StructureAttributes = attributes;
    }

    /// <inheritdoc />
    public override CodeViewLeafKind LeafKind => CodeViewLeafKind.Enum;
}
