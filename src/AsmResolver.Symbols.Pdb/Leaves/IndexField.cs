namespace AsmResolver.Symbols.Pdb.Leaves;

/// <summary>
/// Represents a continuation index within a field list, used when a field list exceeds the maximum record size.
/// </summary>
public partial class IndexField : CodeViewField
{
    /// <summary>
    /// Initializes an empty index field.
    /// </summary>
    /// <param name="typeIndex">The type index to assign to the field.</param>
    protected IndexField(uint typeIndex)
        : base(typeIndex)
    {
    }

    /// <summary>
    /// Creates a new index field.
    /// </summary>
    /// <param name="referencedList">The referenced continuation field list.</param>
    public IndexField(ITpiLeaf referencedList)
        : base(0)
    {
        ReferencedList = referencedList;
    }

    /// <inheritdoc />
    public override CodeViewLeafKind LeafKind => CodeViewLeafKind.Index;

    /// <summary>
    /// Gets or sets the continuation field list that this index references.
    /// </summary>
    [LazyProperty]
    public partial ITpiLeaf? ReferencedList
    {
        get;
        set;
    }

    /// <summary>
    /// Obtains the continuation field list that this index references.
    /// </summary>
    /// <returns>The referenced list.</returns>
    /// <remarks>
    /// This method is called upon initialization of the <see cref="ReferencedList"/> property.
    /// </remarks>
    protected virtual ITpiLeaf? GetReferencedList() => null;

    /// <inheritdoc />
    public override string ToString() => $"Index -> {ReferencedList?.TypeIndex:X8}";
}
