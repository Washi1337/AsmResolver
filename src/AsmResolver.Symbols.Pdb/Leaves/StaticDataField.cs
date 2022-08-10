namespace AsmResolver.Symbols.Pdb.Leaves;

/// <summary>
/// Represents a static data member in a class or structure type.
/// </summary>
public class StaticDataField : CodeViewDataField
{
    /// <summary>
    /// Initializes an empty static data field.
    /// </summary>
    /// <param name="typeIndex">The type index to assign to the field.</param>
    protected StaticDataField(uint typeIndex)
        : base(typeIndex)
    {
    }

    /// <summary>
    /// Creates a new static data member.
    /// </summary>
    /// <param name="dataType">The data type of the member.</param>
    /// <param name="name">The name.</param>
    public StaticDataField(CodeViewTypeRecord dataType, Utf8String name)
        : base(dataType, name)
    {
    }

    /// <inheritdoc />
    public override CodeViewLeafKind LeafKind => CodeViewLeafKind.StMember;
}
