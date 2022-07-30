namespace AsmResolver.Symbols.Pdb.Leaves;

/// <summary>
/// Provides a base for all code view types that can define one or more fields.
/// </summary>
public abstract class CodeViewCompositeType : CodeViewType
{
    private readonly LazyVariable<Utf8String> _name;
    private readonly LazyVariable<FieldList?> _fields;

    /// <summary>
    /// Initializes a new empty composite type.
    /// </summary>
    /// <param name="typeIndex">The type index to assign to the type.</param>
    protected CodeViewCompositeType(uint typeIndex)
        : base(typeIndex)
    {
        _name = new LazyVariable<Utf8String>(GetName);
        _fields = new LazyVariable<FieldList?>(GetFields);
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
    /// Gets or sets the name of the enum type.
    /// </summary>
    public Utf8String Name
    {
        get => _name.Value;
        set => _name.Value = value;
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
    /// Obtains the fields defined in the type.
    /// </summary>
    /// <returns>The fields.</returns>
    /// <remarks>
    /// This method is called upon initialization of the <see cref="Fields"/> property.
    /// </remarks>
    protected virtual FieldList? GetFields() => null;
}
