using System;

namespace AsmResolver.Symbols.Pdb.Leaves;

/// <summary>
/// Represents a class, structure or interface type in a PDB.
/// </summary>
public class ClassType : CodeViewDerivedType
{
    private readonly LazyVariable<Utf8String> _uniqueName;
    private readonly LazyVariable<VTableShape?> _vtableShape;

    /// <summary>
    /// Initializes an empty class type.
    /// </summary>
    /// <param name="kind">The kind of type.</param>
    /// <param name="typeIndex">The type index to assign to the class type.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Occurs when the provided kind is not a class, structure or interface.
    /// </exception>
    protected ClassType(CodeViewLeafKind kind, uint typeIndex)
        : base(typeIndex)
    {
        if (kind is not (CodeViewLeafKind.Class or CodeViewLeafKind.Structure or CodeViewLeafKind.Interface))
            throw new ArgumentOutOfRangeException(nameof(kind));

        LeafKind = kind;
        _uniqueName = new LazyVariable<Utf8String>(GetUniqueName);
        _vtableShape = new LazyVariable<VTableShape?>(GetVTableShape);
    }

    /// <summary>
    /// Creates a new class type record.
    /// </summary>
    /// <param name="kind">The kind.</param>
    /// <param name="name">The name of the type.</param>
    /// <param name="uniqueName">The unique mangled name of the type.</param>
    /// <param name="size">The size in bytes of the type.</param>
    /// <param name="attributes">Attributes describing the shape of the type.</param>
    /// <param name="baseType">The type that this type is derived from, if any.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Occurs when the provided kind is not a class, structure or interface.
    /// </exception>
    public ClassType(CodeViewLeafKind kind, Utf8String name, Utf8String uniqueName, ulong size,
        StructureAttributes attributes, CodeViewType? baseType)
        : base(0)
    {
        if (kind is not (CodeViewLeafKind.Class or CodeViewLeafKind.Structure or CodeViewLeafKind.Interface))
            throw new ArgumentOutOfRangeException(nameof(kind));

        LeafKind = kind;
        Name = name;
        _uniqueName = new LazyVariable<Utf8String>(uniqueName);
        _vtableShape = new LazyVariable<VTableShape?>(default(VTableShape));
        Size = size;
        StructureAttributes = attributes;
        BaseType = baseType;
    }

    /// <inheritdoc />
    public override CodeViewLeafKind LeafKind
    {
        get;
    }

    /// <summary>
    /// Gets or sets the number bytes that this class spans.
    /// </summary>
    public ulong Size
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the uniquely identifiable name for this type.
    /// </summary>
    public Utf8String UniqueName
    {
        get => _uniqueName.Value;
        set => _uniqueName.Value = value;
    }

    /// <summary>
    /// Gets or sets the shape of the virtual function table of this type, if available.
    /// </summary>
    public VTableShape? VTableShape
    {
        get => _vtableShape.Value;
        set => _vtableShape.Value = value;
    }

    /// <summary>
    /// Obtains the uniquely identifiable name of the type.
    /// </summary>
    /// <returns>The name.</returns>
    /// <remarks>
    /// This method is called upon initialization of the <see cref="UniqueName"/> property.
    /// </remarks>
    protected virtual Utf8String GetUniqueName() => Utf8String.Empty;

    /// <summary>
    /// Obtains the shape of the virtual function table name of the type.
    /// </summary>
    /// <returns>The shape.</returns>
    /// <remarks>
    /// This method is called upon initialization of the <see cref="VTableShape"/> property.
    /// </remarks>
    protected virtual VTableShape? GetVTableShape() => null;
}
