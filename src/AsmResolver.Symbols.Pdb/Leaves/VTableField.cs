namespace AsmResolver.Symbols.Pdb.Leaves;

/// <summary>
/// Represents the virtual function table field in a class.
/// </summary>
public partial class VTableField : CodeViewField
{
    /// <summary>
    /// Initializes an empty virtual function table field.
    /// </summary>
    /// <param name="typeIndex">The type index to assign to the type.</param>
    protected VTableField(uint typeIndex)
        : base(typeIndex)
    {
    }

    /// <summary>
    /// Creates a new virtual function table field.
    /// </summary>
    /// <param name="pointerType">The pointer type to use.</param>
    public VTableField(CodeViewTypeRecord pointerType)
        : base(0)
    {
        PointerType = pointerType;
    }

    /// <inheritdoc />
    public override CodeViewLeafKind LeafKind => CodeViewLeafKind.VFuncTab;

    /// <summary>
    /// Gets or sets the pointer type of the virtual function table.
    /// </summary>
    [LazyProperty]
    public partial CodeViewTypeRecord? PointerType
    {
        get;
        set;
    }

    /// <summary>
    /// Obtains the pointer type that the virtual function table type.
    /// </summary>
    /// <returns>The pointer type.</returns>
    /// <remarks>
    /// This method is called upon initialization of the <see cref="PointerType"/> property.
    /// </remarks>
    protected virtual CodeViewTypeRecord? GetPointerType() => null;
}
