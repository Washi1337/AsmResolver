namespace AsmResolver.Symbols.Pdb.Leaves;

/// <summary>
/// Provides a base for all custom types that may define fields and/or are derived from a base type.
/// </summary>
public abstract class CodeViewComplexType : CodeViewType
{
    private readonly LazyVariable<Utf8String> _name;
    private readonly LazyVariable<CodeViewType?> _baseType;
    private readonly LazyVariable<FieldList?> _fields;

    /// <inheritdoc />
    protected CodeViewComplexType(uint typeIndex)
        : base(typeIndex)
    {
        _name = new LazyVariable<Utf8String>(GetName);
        _baseType = new LazyVariable<CodeViewType?>(GetBaseType);
        _fields = new LazyVariable<FieldList?>(GetFields);
    }

    /// <summary>
    /// Gets or sets the name of the enum type.
    /// </summary>
    public Utf8String Name
    {
        get => _name.Value;
        set => _name.Value = value;
    }

    /// <summary>
    /// Gets or sets the structural attributes assigned to the type.
    /// </summary>
    public StructureAttributes StructureAttributes
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the base type that this type is deriving from.
    /// </summary>
    public CodeViewType? BaseType
    {
        get => _baseType.Value;
        set => _baseType.Value = value;
    }

    /// <summary>
    /// Gets a collection of fields that are defined in the enum.
    /// </summary>
    public FieldList? Fields
    {
        get => _fields.Value;
        set => _fields.Value = value;
    }

    /// <summary>
    /// Obtains the new name of the type.
    /// </summary>
    /// <returns>The name.</returns>
    /// <remarks>
    /// This method is called upon initialization of the <see cref="Name"/> property.
    /// </remarks>
    protected virtual Utf8String GetName() => Utf8String.Empty;

    /// <summary>
    /// Obtains the type that the type is derived from.
    /// </summary>
    /// <returns>The type.</returns>
    /// <remarks>
    /// This method is called upon initialization of the <see cref="BaseType"/> property.
    /// </remarks>
    protected virtual CodeViewType? GetBaseType() => null;

    /// <summary>
    /// Obtains the fields defined in the type.
    /// </summary>
    /// <returns>The fields.</returns>
    /// <remarks>
    /// This method is called upon initialization of the <see cref="Fields"/> property.
    /// </remarks>
    protected virtual FieldList? GetFields() => null;

    /// <inheritdoc />
    public override string ToString() => Name;
}
