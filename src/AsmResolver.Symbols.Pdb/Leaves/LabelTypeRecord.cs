namespace AsmResolver.Symbols.Pdb.Leaves;

/// <summary>
/// Represents an assembler label type record.
/// </summary>
public class LabelTypeRecord : CodeViewTypeRecord
{
    /// <summary>
    /// Initializes an empty label type record.
    /// </summary>
    /// <param name="typeIndex">The type index to assign to the type.</param>
    protected LabelTypeRecord(uint typeIndex)
        : base(typeIndex)
    {
    }

    /// <summary>
    /// Creates a new label type record.
    /// </summary>
    /// <param name="mode">The addressing mode of the label.</param>
    public LabelTypeRecord(LabelAddressingMode mode)
        : base(0)
    {
        Mode = mode;
    }

    /// <inheritdoc />
    public override CodeViewLeafKind LeafKind => CodeViewLeafKind.Label;

    /// <summary>
    /// Gets or sets the addressing mode of the label.
    /// </summary>
    public LabelAddressingMode Mode
    {
        get;
        set;
    }

    /// <inheritdoc />
    public override string ToString() => $"Label ({Mode})";
}
