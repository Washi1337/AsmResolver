namespace AsmResolver.Symbols.Pdb.Leaves;

/// <summary>
/// Represents a field in a type that references a nested type definition.
/// </summary>
public partial class NestedTypeField : CodeViewNamedField
{
    private readonly object _lock = new();

    /// <summary>
    /// Initializes an empty nested type.
    /// </summary>
    /// <param name="typeIndex">The type index to assign to the nested type field.</param>
    protected NestedTypeField(uint typeIndex)
        : base(typeIndex)
    {
    }

    /// <summary>
    /// Creates a new nested type field.
    /// </summary>
    /// <param name="nestedType">The definition of the nested type</param>
    /// <param name="name">The name of the nested type.</param>
    public NestedTypeField(CodeViewTypeRecord nestedType, Utf8String name)
        : base(0)
    {
        NestedType = nestedType;
        Name = name;
        Attributes = 0;
    }

    /// <summary>
    /// Creates a new nested type (extended) field.
    /// </summary>
    /// <param name="nestedType">The definition of the nested type</param>
    /// <param name="name">The name of the nested type.</param>
    /// <param name="attributes">The attributes assigned to the type.</param>
    public NestedTypeField(CodeViewTypeRecord nestedType, Utf8String name, CodeViewFieldAttributes attributes)
        : base(0)
    {
        NestedType = nestedType;
        Name = name;
        Attributes = attributes;
    }

    /// <inheritdoc />
    public override CodeViewLeafKind LeafKind => Attributes == 0
        ? CodeViewLeafKind.NestType
        : CodeViewLeafKind.NestTypeEx;

    /// <summary>
    /// Gets or sets the definition of the referenced nested type.
    /// </summary>
    [LazyProperty]
    public partial CodeViewTypeRecord? NestedType
    {
        get;
        set;
    }

    /// <summary>
    /// Obtains the definition of the nested type.
    /// </summary>
    /// <returns>The type</returns>
    /// <remarks>
    /// This method is called upon initialization of the <see cref="NestedType"/> property.
    /// </remarks>
    protected virtual CodeViewTypeRecord? GetNestedType() => null;
}
