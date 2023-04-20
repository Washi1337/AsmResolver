namespace AsmResolver.Symbols.Pdb.Leaves;

/// <summary>
/// Represents a direct or indirect reference to a virtual base class object in a structure.
/// </summary>
public class VBaseClassField : CodeViewField
{
    private readonly LazyVariable<VBaseClassField, CodeViewTypeRecord?> _baseType;
    private readonly LazyVariable<VBaseClassField, CodeViewTypeRecord?> _basePointerType;

    /// <summary>
    /// Initializes a new empty virtual base class field.
    /// </summary>
    /// <param name="typeIndex">The type index to assign to the field.</param>
    protected VBaseClassField(uint typeIndex)
        : base(typeIndex)
    {
        _baseType = new LazyVariable<VBaseClassField, CodeViewTypeRecord?>(x => x.GetBaseType());
        _basePointerType = new LazyVariable<VBaseClassField, CodeViewTypeRecord?>(x => x.GetBasePointerType());
    }

    /// <summary>
    /// Creates a new virtual base class field.
    /// </summary>
    /// <param name="baseType">The type to reference as base type.</param>
    /// <param name="pointerType">The type of the virtual base pointer.</param>
    /// <param name="pointerOffset">The offset of the virtual base pointer</param>
    /// <param name="tableOffset">The offset from the base table.</param>
    /// <param name="isIndirect"><c>true</c> if the field is an indirect virtual base class, <c>false</c> otherwise.</param>
    public VBaseClassField(
        CodeViewTypeRecord baseType,
        CodeViewTypeRecord pointerType,
        ulong pointerOffset,
        ulong tableOffset,
        bool isIndirect)
        : base(0)
    {
        _baseType = new LazyVariable<VBaseClassField, CodeViewTypeRecord?>(baseType);
        _basePointerType = new LazyVariable<VBaseClassField, CodeViewTypeRecord?>(pointerType);
        PointerOffset = pointerOffset;
        TableOffset = tableOffset;
        IsIndirect = isIndirect;
    }

    /// <summary>
    /// Gets or sets a value indicating whether the virtual base class is an indirect base class.
    /// </summary>
    public bool IsIndirect
    {
        get;
        set;
    }

    /// <inheritdoc />
    public override CodeViewLeafKind LeafKind => IsIndirect
        ? CodeViewLeafKind.IVBClass
        : CodeViewLeafKind.VBClass;

    /// <summary>
    /// Gets or sets the base type that this base class is referencing.
    /// </summary>
    public CodeViewTypeRecord? Type
    {
        get => _baseType.GetValue(this);
        set => _baseType.SetValue(value);
    }

    /// <summary>
    /// Gets or sets the type of the base pointer that this base class uses.
    /// </summary>
    public CodeViewTypeRecord? PointerType
    {
        get => _basePointerType.GetValue(this);
        set => _basePointerType.SetValue(value);
    }

    /// <summary>
    /// Gets or sets the virtual base pointer offset relative to the address point.
    /// </summary>
    public ulong PointerOffset
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the virtual base pointer offset relative to the virtual base table.
    /// </summary>
    public ulong TableOffset
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
    protected virtual CodeViewTypeRecord? GetBaseType() => null;

    /// <summary>
    /// Obtains the type of the base pointer that the class is uses.
    /// </summary>
    /// <returns>The base pointer type.</returns>
    /// <remarks>
    /// This method is called upon initialization of the <see cref="PointerType"/> property.
    /// </remarks>
    protected virtual CodeViewTypeRecord? GetBasePointerType() => null;

    /// <inheritdoc />
    public override string ToString() => Type?.ToString() ?? "<<<?>>>";
}
