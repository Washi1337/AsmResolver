namespace AsmResolver.Symbols.Pdb.Leaves;

/// <summary>
/// Represents an enum type.
/// </summary>
public class EnumType : CodeViewType
{
    private readonly LazyVariable<Utf8String> _name;
    private readonly LazyVariable<CodeViewLeaf> _type;
    private readonly LazyVariable<FieldList> _fields;

    /// <summary>
    /// Initializes a new empty enum type.
    /// </summary>
    /// <param name="typeIndex">The type index to assign to the enum type.</param>
    protected EnumType(uint typeIndex)
        : base(typeIndex)
    {
        _name = new LazyVariable<Utf8String>(GetName);
        _type = new LazyVariable<CodeViewLeaf>(GetEnumUnderlyingType);
        _fields = new LazyVariable<FieldList>(GetFields);
    }

    /// <summary>
    /// Creates a new enum type.
    /// </summary>
    /// <param name="name">The name of the enum.</param>
    /// <param name="underlyingType">The underlying type of all members in the enum.</param>
    /// <param name="attributes">The structural attributes assigned to the enum.</param>
    public EnumType(Utf8String name, CodeViewLeaf underlyingType, StructureAttributes attributes)
        : base(0)
    {
        _name = new LazyVariable<Utf8String>(name);
        _type = new LazyVariable<CodeViewLeaf>(underlyingType);
        _fields = new LazyVariable<FieldList>(new FieldList());
        StructureAttributes = attributes;
    }

    /// <inheritdoc />
    public override CodeViewLeafKind LeafKind => CodeViewLeafKind.Enum;

    /// <summary>
    /// Gets or sets the structural attributes assigned to the enum.
    /// </summary>
    public StructureAttributes StructureAttributes
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the underlying type of all members in the enum.
    /// </summary>
    public CodeViewLeaf EnumUnderlyingType
    {
        get => _type.Value;
        set => _type.Value = value;
    }

    /// <summary>
    /// Gets a collection of fields that are defined in the enum.
    /// </summary>
    public FieldList Fields => _fields.Value;

    /// <summary>
    /// Gets or sets the name of the enum type.
    /// </summary>
    public Utf8String Name
    {
        get => _name.Value;
        set => _name.Value = value;
    }

    /// <summary>
    /// Obtains the new name of the enum type.
    /// </summary>
    /// <returns>The name.</returns>
    /// <remarks>
    /// This method is called upon initialization of the <see cref="Name"/> property.
    /// </remarks>
    protected virtual Utf8String GetName() => Utf8String.Empty;

    /// <summary>
    /// Obtains the type that the enum is based on.
    /// </summary>
    /// <returns>The type.</returns>
    /// <remarks>
    /// This method is called upon initialization of the <see cref="EnumUnderlyingType"/> property.
    /// </remarks>
    protected virtual CodeViewLeaf? GetEnumUnderlyingType() => null;

    /// <summary>
    /// Obtains the fields defined in the enum type.
    /// </summary>
    /// <returns>The fields.</returns>
    /// <remarks>
    /// This method is called upon initialization of the <see cref="Fields"/> property.
    /// </remarks>
    protected virtual FieldList GetFields() => new();

    /// <inheritdoc />
    public override string ToString() => Name;
}
