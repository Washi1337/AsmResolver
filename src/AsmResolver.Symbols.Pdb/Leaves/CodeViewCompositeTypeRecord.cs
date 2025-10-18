namespace AsmResolver.Symbols.Pdb.Leaves;

/// <summary>
/// Provides a base for all code view types that can define one or more fields.
/// </summary>
public abstract partial class CodeViewCompositeTypeRecord : CodeViewTypeRecord
{
    /// <summary>
    /// Initializes a new empty composite type.
    /// </summary>
    /// <param name="typeIndex">The type index to assign to the type.</param>
    protected CodeViewCompositeTypeRecord(uint typeIndex)
        : base(typeIndex)
    {
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
    [LazyProperty]
    public partial Utf8String Name
    {
        get;
        set;
    }

    /// <summary>
    /// Gets a collection of fields that are defined in the enum.
    /// </summary>
    [LazyProperty]
    public partial FieldListLeaf? Fields
    {
        get;
        set;
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
