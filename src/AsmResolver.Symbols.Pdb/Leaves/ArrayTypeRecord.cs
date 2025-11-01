namespace AsmResolver.Symbols.Pdb.Leaves;

/// <summary>
/// Represents a type describing an array of elements.
/// </summary>
public partial class ArrayTypeRecord : CodeViewTypeRecord
{
    /// <summary>
    /// Initializes a new empty array type.
    /// </summary>
    /// <param name="typeIndex">The type index to assign to the type.</param>
    protected ArrayTypeRecord(uint typeIndex)
        : base(typeIndex)
    {
    }

    /// <summary>
    /// Creates a new array type.
    /// </summary>
    /// <param name="elementType">The type of each element in the array.</param>
    /// <param name="indexType">The type to use for indexing into the array.</param>
    /// <param name="length">The number of elements in the array.</param>
    public ArrayTypeRecord(CodeViewTypeRecord elementType, CodeViewTypeRecord indexType, ulong length)
        : base(0)
    {
        ElementType = elementType;
        IndexType = indexType;
        Name = Utf8String.Empty;
        Length = length;
    }

    /// <summary>
    /// Creates a new array type.
    /// </summary>
    /// <param name="elementType">The type of each element in the array.</param>
    /// <param name="indexType">The type to use for indexing into the array.</param>
    /// <param name="length">The number of elements in the array.</param>
    /// <param name="name">The name of the array type.</param>
    public ArrayTypeRecord(CodeViewTypeRecord elementType, CodeViewTypeRecord indexType, ulong length, Utf8String name)
        : base(0)
    {
        ElementType = elementType;
        IndexType = indexType;
        Name = name;
        Length = length;
    }

    /// <inheritdoc />
    public override CodeViewLeafKind LeafKind => CodeViewLeafKind.Array;

    /// <summary>
    /// Gets or sets the type of each element in the array.
    /// </summary>
    [LazyProperty]
    public partial CodeViewTypeRecord? ElementType
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the type that is used to index into the array.
    /// </summary>
    [LazyProperty]
    public partial CodeViewTypeRecord? IndexType
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the number of elements in the array.
    /// </summary>
    public ulong Length
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the name of the type.
    /// </summary>
    [LazyProperty]
    public partial Utf8String Name
    {
        get;
        set;
    }

    /// <summary>
    /// Obtains the element type of the array.
    /// </summary>
    /// <returns>The element type.</returns>
    /// <remarks>
    /// This method is called upon initialization of the <see cref="ElementType"/> property.
    /// </remarks>
    protected virtual CodeViewTypeRecord? GetElementType() => null;

    /// <summary>
    /// Obtains the index type of the array.
    /// </summary>
    /// <returns>The index type.</returns>
    /// <remarks>
    /// This method is called upon initialization of the <see cref="IndexType"/> property.
    /// </remarks>
    protected virtual CodeViewTypeRecord? GetIndexType() => null;

    /// <summary>
    /// Obtains the name type of the array.
    /// </summary>
    /// <returns>The name.</returns>
    /// <remarks>
    /// This method is called upon initialization of the <see cref="Name"/> property.
    /// </remarks>
    protected virtual Utf8String GetName() => Utf8String.Empty;

    /// <inheritdoc />
    public override string ToString() => $"{ElementType}[{Length}]";
}
