namespace AsmResolver.Symbols.Pdb.Leaves;

/// <summary>
/// Represents a type describing an array of elements.
/// </summary>
public class ArrayTypeRecord : CodeViewTypeRecord
{
    private readonly LazyVariable<ArrayTypeRecord, CodeViewTypeRecord?> _elementType;
    private readonly LazyVariable<ArrayTypeRecord, CodeViewTypeRecord?> _indexType;
    private readonly LazyVariable<ArrayTypeRecord, Utf8String> _name;

    /// <summary>
    /// Initializes a new empty array type.
    /// </summary>
    /// <param name="typeIndex">The type index to assign to the type.</param>
    protected ArrayTypeRecord(uint typeIndex)
        : base(typeIndex)
    {
        _elementType = new LazyVariable<ArrayTypeRecord, CodeViewTypeRecord?>(x => x.GetElementType());
        _indexType = new LazyVariable<ArrayTypeRecord, CodeViewTypeRecord?>(x => x.GetIndexType());
        _name = new LazyVariable<ArrayTypeRecord, Utf8String>(x => x.GetName());
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
        _elementType = new LazyVariable<ArrayTypeRecord, CodeViewTypeRecord?>(elementType);
        _indexType = new LazyVariable<ArrayTypeRecord, CodeViewTypeRecord?>(indexType);
        _name = new LazyVariable<ArrayTypeRecord, Utf8String>(Utf8String.Empty);
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
        _elementType = new LazyVariable<ArrayTypeRecord, CodeViewTypeRecord?>(elementType);
        _indexType = new LazyVariable<ArrayTypeRecord, CodeViewTypeRecord?>(indexType);
        _name = new LazyVariable<ArrayTypeRecord, Utf8String>(name);
        Length = length;
    }

    /// <inheritdoc />
    public override CodeViewLeafKind LeafKind => CodeViewLeafKind.Array;

    /// <summary>
    /// Gets or sets the type of each element in the array.
    /// </summary>
    public CodeViewTypeRecord? ElementType
    {
        get => _elementType.GetValue(this);
        set => _elementType.SetValue(value);
    }

    /// <summary>
    /// Gets or sets the type that is used to index into the array.
    /// </summary>
    public CodeViewTypeRecord? IndexType
    {
        get => _indexType.GetValue(this);
        set => _indexType.SetValue(value);
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
    public Utf8String Name
    {
        get => _name.GetValue(this);
        set => _name.SetValue(value);
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
