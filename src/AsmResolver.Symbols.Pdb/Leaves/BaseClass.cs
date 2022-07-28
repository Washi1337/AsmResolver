namespace AsmResolver.Symbols.Pdb.Leaves;

/// <summary>
/// Represents a reference to a base class object in a structure.
/// </summary>
public class BaseClass : CodeViewField
{
    private readonly LazyVariable<CodeViewType?> _type;

    /// <summary>
    /// Initializes an empty base class.
    /// </summary>
    /// <param name="typeIndex">The type index to assign to the base class field.</param>
    protected BaseClass(uint typeIndex)
        : base(typeIndex)
    {
        _type = new LazyVariable<CodeViewType?>(GetBaseType);
    }

    /// <inheritdoc />
    public override CodeViewLeafKind LeafKind => CodeViewLeafKind.BClass;

    /// <summary>
    /// Gets or sets the base type that this base class is referencing.
    /// </summary>
    public CodeViewType? Type
    {
        get => _type.Value;
        set => _type.Value = value;
    }

    /// <summary>
    /// Gets or sets the offset of the base within the class.
    /// </summary>
    public ulong Offset
    {
        get;
        set;
    }

    /// <summary>
    /// Obtains the base type that the class is referencing.
    /// </summary>
    /// <returns>The base type.</returns>
    /// <remarks>
    /// This method is called upon initialization of the <see cref="Type"/> property.
    /// </remarks>
    protected virtual CodeViewType? GetBaseType() => null;

    /// <inheritdoc />
    public override string ToString() => Type?.ToString() ?? "<<<?>>>";
}
