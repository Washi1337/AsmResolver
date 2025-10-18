namespace AsmResolver.Symbols.Pdb.Leaves;

/// <summary>
/// Represents a String ID entry within the IPI stream of a PDB image.
/// </summary>
public partial class StringIdentifier : CodeViewLeaf, IIpiLeaf
{
    /// <summary>
    /// Initializes an empty String ID entry.
    /// </summary>
    /// <param name="typeIndex">The type index.</param>
    protected StringIdentifier(uint typeIndex)
        : base(typeIndex)
    {
    }

    /// <summary>
    /// Creates a new String ID entry.
    /// </summary>
    /// <param name="value">The string value to wrap.</param>
    public StringIdentifier(Utf8String value)
        : this(value, null)
    {
    }

    /// <summary>
    /// Creates a new String ID entry.
    /// </summary>
    /// <param name="value">The string value to wrap.</param>
    /// <param name="subStrings">A list of sub strings.</param>
    public StringIdentifier(Utf8String value, SubStringListLeaf? subStrings)
        : base(0)
    {
        Value = value;
        SubStrings = subStrings;
    }

    /// <inheritdoc />
    public override CodeViewLeafKind LeafKind => CodeViewLeafKind.StringId;

    /// <summary>
    /// Gets or sets the wrapped string.
    /// </summary>
    [LazyProperty]
    public partial Utf8String Value
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets a list of sub strings associated to the entry (if available).
    /// </summary>
    [LazyProperty]
    public partial SubStringListLeaf? SubStrings
    {
        get;
        set;
    }

    /// <summary>
    /// Obtains the wrapped string value.
    /// </summary>
    /// <returns>The string.</returns>
    /// <remarks>
    /// This method is called upon initialization of the <see cref="Value"/> property.
    /// </remarks>
    protected virtual Utf8String? GetValue() => null;

    /// <summary>
    /// Obtains the sub strings associated to the string.
    /// </summary>
    /// <returns>The sub string.</returns>
    /// <remarks>
    /// This method is called upon initialization of the <see cref="SubStrings"/> property.
    /// </remarks>
    protected virtual SubStringListLeaf? GetSubStrings() => null;

    /// <inheritdoc />
    public override string ToString()
    {
        return SubStrings is not null
            ? $"{Value} (Contains sub strings)"
            : Value;
    }
}
