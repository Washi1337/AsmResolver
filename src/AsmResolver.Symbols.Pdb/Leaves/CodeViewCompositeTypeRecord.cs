namespace AsmResolver.Symbols.Pdb.Leaves;

/// <summary>
/// Provides a base for all code view types that can define one or more fields.
/// </summary>
public abstract class CodeViewCompositeTypeRecord : CodeViewTypeRecord
{
    private readonly LazyVariable<CodeViewCompositeTypeRecord, Utf8String> _name;
    private readonly LazyVariable<CodeViewCompositeTypeRecord, FieldListLeaf?> _fields;

    /// <summary>
    /// Initializes a new empty composite type.
    /// </summary>
    /// <param name="typeIndex">The type index to assign to the type.</param>
    protected CodeViewCompositeTypeRecord(uint typeIndex)
        : base(typeIndex)
    {
        _name = new LazyVariable<CodeViewCompositeTypeRecord, Utf8String>(x => x.GetName());
        _fields = new LazyVariable<CodeViewCompositeTypeRecord, FieldListLeaf?>(x => x.GetFields());
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
    /// Gets or sets the name of the type.
    /// </summary>
    public Utf8String Name
    {
        get => _name.GetValue(this);
        set => _name.SetValue(value);
    }

    /// <summary>
    /// Gets a collection of fields that are defined in the enum.
    /// </summary>
    public FieldListLeaf? Fields
    {
        get => _fields.GetValue(this);
        set => _fields.SetValue(value);
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
    protected virtual FieldListLeaf? GetFields() => null;
}
