using System;

namespace AsmResolver.Symbols.Pdb.Leaves;

/// <summary>
/// Represents a class, structure or interface type in a PDB.
/// </summary>
public class ClassTypeRecord : CodeViewDerivedTypeRecord
{
    private readonly LazyVariable<ClassTypeRecord, Utf8String> _uniqueName;
    private readonly LazyVariable<ClassTypeRecord, VTableShapeLeaf?> _vtableShape;

    /// <summary>
    /// Initializes an empty class type.
    /// </summary>
    /// <param name="kind">The kind of type.</param>
    /// <param name="typeIndex">The type index to assign to the class type.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Occurs when the provided kind is not a class, structure or interface.
    /// </exception>
    protected ClassTypeRecord(CodeViewLeafKind kind, uint typeIndex)
        : base(typeIndex)
    {
        if (kind is not (CodeViewLeafKind.Class or CodeViewLeafKind.Structure or CodeViewLeafKind.Interface))
            throw new ArgumentOutOfRangeException(nameof(kind));

        LeafKind = kind;
        _uniqueName = new LazyVariable<ClassTypeRecord, Utf8String>(x => x.GetUniqueName());
        _vtableShape = new LazyVariable<ClassTypeRecord, VTableShapeLeaf?>(x => x.GetVTableShape());
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
    public ClassTypeRecord(CodeViewLeafKind kind, Utf8String name, Utf8String uniqueName, ulong size,
        StructureAttributes attributes, CodeViewTypeRecord? baseType)
        : base(0)
    {
        if (kind is not (CodeViewLeafKind.Class or CodeViewLeafKind.Structure or CodeViewLeafKind.Interface))
            throw new ArgumentOutOfRangeException(nameof(kind));

        LeafKind = kind;
        Name = name;
        _uniqueName = new LazyVariable<ClassTypeRecord, Utf8String>(uniqueName);
        _vtableShape = new LazyVariable<ClassTypeRecord, VTableShapeLeaf?>(default(VTableShapeLeaf));
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
        get => _uniqueName.GetValue(this);
        set => _uniqueName.SetValue(value);
    }

    /// <summary>
    /// Gets or sets the shape of the virtual function table of this type, if available.
    /// </summary>
    public VTableShapeLeaf? VTableShape
    {
        get => _vtableShape.GetValue(this);
        set => _vtableShape.SetValue(value);
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
    protected virtual VTableShapeLeaf? GetVTableShape() => null;
}
