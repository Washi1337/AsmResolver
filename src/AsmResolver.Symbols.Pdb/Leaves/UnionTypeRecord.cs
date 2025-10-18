namespace AsmResolver.Symbols.Pdb.Leaves;

/// <summary>
/// Represents a type of a value that may have several representations or formats within the same position of memory.
/// </summary>
public partial class UnionTypeRecord : CodeViewCompositeTypeRecord
{
    private readonly object _lock = new();

    /// <summary>
    /// Initializes an empty union type.
    /// </summary>
    /// <param name="typeIndex">The type index to assign to the union.</param>
    protected UnionTypeRecord(uint typeIndex)
        : base(typeIndex)
    {
    }

    /// <summary>
    /// Creates a new union type with the provided size.
    /// </summary>
    /// <param name="size">The total size in bytes of the union.</param>
    public UnionTypeRecord(ulong size)
        : base(0)
    {
        UniqueName = Utf8String.Empty;
        Size = size;
    }

    /// <inheritdoc />
    public override CodeViewLeafKind LeafKind => CodeViewLeafKind.Union;

    /// <summary>
    /// Gets or sets the total size in bytes of the union type.
    /// </summary>
    public ulong Size
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the uniquely identifiable name for this type.
    /// </summary>
    [LazyProperty]
    public partial Utf8String UniqueName
    {
        get;
        set;
    }

    /// <summary>
    /// Obtains the uniquely identifiable name of the type.
    /// </summary>
    /// <returns>The name.</returns>
    /// <remarks>
    /// This method is called upon initialization of the <see cref="UniqueName"/> property.
    /// </remarks>
    protected virtual Utf8String? GetUniqueName() => null;

    /// <inheritdoc />
    public override string ToString()
    {
        if (!Utf8String.IsNullOrEmpty(Name))
            return Name;
        if (!Utf8String.IsNullOrEmpty(UniqueName))
            return UniqueName;
        return $"<unnamed-union ({Size} bytes)>";
    }
}
