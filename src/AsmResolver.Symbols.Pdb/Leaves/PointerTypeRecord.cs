namespace AsmResolver.Symbols.Pdb.Leaves;

/// <summary>
/// Represents a pointer type in a TPI or IPI stream.
/// </summary>
public partial class PointerTypeRecord : CodeViewTypeRecord
{
    /// <summary>
    /// Initializes a new empty pointer type.
    /// </summary>
    /// <param name="typeIndex">The type index to assign to the type.</param>
    protected PointerTypeRecord(uint typeIndex)
        : base(typeIndex)
    {
    }

    /// <summary>
    /// Creates a new pointer type.
    /// </summary>
    /// <param name="type">The referent type.</param>
    /// <param name="attributes">The attributes describing the shape of the pointer.</param>
    public PointerTypeRecord(CodeViewTypeRecord type, PointerAttributes attributes)
        : base(0)
    {
        BaseType = type;
        Attributes = attributes;
    }

    /// <summary>
    /// Creates a new pointer type.
    /// </summary>
    /// <param name="type">The referent type.</param>
    /// <param name="attributes">The attributes describing the shape of the pointer.</param>
    /// <param name="size">The size of the pointer.</param>
    public PointerTypeRecord(CodeViewTypeRecord type, PointerAttributes attributes, byte size)
        : base(0)
    {
        BaseType = type;
        Attributes = attributes;
        Size = size;
    }

    /// <inheritdoc />
    public override CodeViewLeafKind LeafKind => CodeViewLeafKind.Pointer;

    /// <summary>
    /// Gets or sets the referent type of the pointer.
    /// </summary>
    [LazyProperty]
    public partial CodeViewTypeRecord BaseType
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the attributes describing the shape of the pointer type.
    /// </summary>
    public PointerAttributes Attributes
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the kind of the pointer.
    /// </summary>
    public PointerAttributes Kind
    {
        get => Attributes & PointerAttributes.KindMask;
        set => Attributes = (Attributes & ~PointerAttributes.KindMask) | (value & PointerAttributes.KindMask);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the pointer is a 16 bit pointer.
    /// </summary>
    public bool IsNear16
    {
        get => (Attributes & PointerAttributes.KindMask) == PointerAttributes.Near16;
        set => Attributes = (Attributes & ~PointerAttributes.KindMask)
                            | (value ? PointerAttributes.Near16 : 0);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the pointer is a 16:16 far pointer.
    /// </summary>
    public bool IsFar16
    {
        get => (Attributes & PointerAttributes.KindMask) == PointerAttributes.Far16;
        set => Attributes = (Attributes & ~PointerAttributes.KindMask)
                            | (value ? PointerAttributes.Far16 : 0);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the pointer is a 16:16 huge pointer.
    /// </summary>
    public bool IsHuge16
    {
        get => (Attributes & PointerAttributes.KindMask) == PointerAttributes.Huge16;
        set => Attributes = (Attributes & ~PointerAttributes.KindMask)
                            | (value ? PointerAttributes.Huge16 : 0);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the pointer is a based on segment.
    /// </summary>
    public bool IsBasedOnSegment
    {
        get => (Attributes & PointerAttributes.KindMask) == PointerAttributes.BasedOnSegment;
        set => Attributes = (Attributes & ~PointerAttributes.KindMask)
                            | (value ? PointerAttributes.BasedOnSegment : 0);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the pointer is a based on value of base.
    /// </summary>
    public bool IsBasedOnValue
    {
        get => (Attributes & PointerAttributes.KindMask) == PointerAttributes.BasedOnValue;
        set => Attributes = (Attributes & ~PointerAttributes.KindMask)
                            | (value ? PointerAttributes.BasedOnValue : 0);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the pointer is a based on segment value of base.
    /// </summary>
    public bool IsBasedOnSegmentValue
    {
        get => (Attributes & PointerAttributes.KindMask) == PointerAttributes.BasedOnSegmentValue;
        set => Attributes = (Attributes & ~PointerAttributes.KindMask)
                            | (value ? PointerAttributes.BasedOnSegmentValue : 0);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the pointer is a based on address of base.
    /// </summary>
    public bool IsBasedOnAddress
    {
        get => (Attributes & PointerAttributes.KindMask) == PointerAttributes.BasedOnAddress;
        set => Attributes = (Attributes & ~PointerAttributes.KindMask)
                            | (value ? PointerAttributes.BasedOnAddress : 0);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the pointer is a based on segment address of base.
    /// </summary>
    public bool IsBasedOnSegmentAddress
    {
        get => (Attributes & PointerAttributes.KindMask) == PointerAttributes.BasedOnSegmentAddress;
        set => Attributes = (Attributes & ~PointerAttributes.KindMask)
                            | (value ? PointerAttributes.BasedOnSegmentAddress : 0);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the pointer is a based on type.
    /// </summary>
    public bool IsBasedOnType
    {
        get => (Attributes & PointerAttributes.KindMask) == PointerAttributes.BasedOnType;
        set => Attributes = (Attributes & ~PointerAttributes.KindMask)
                            | (value ? PointerAttributes.BasedOnType : 0);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the pointer is a based on self.
    /// </summary>
    public bool IsBasedOnSelf
    {
        get => (Attributes & PointerAttributes.KindMask) == PointerAttributes.BasedOnSelf;
        set => Attributes = (Attributes & ~PointerAttributes.KindMask)
                            | (value ? PointerAttributes.BasedOnSelf : 0);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the pointer is a 32 bit pointer.
    /// </summary>
    public bool IsNear32
    {
        get => (Attributes & PointerAttributes.KindMask) == PointerAttributes.Near32;
        set => Attributes = (Attributes & ~PointerAttributes.KindMask)
                            | (value ? PointerAttributes.Near32 : 0);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the pointer is a 16:32 pointer.
    /// </summary>
    public bool IsFar32
    {
        get => (Attributes & PointerAttributes.KindMask) == PointerAttributes.Far32;
        set => Attributes = (Attributes & ~PointerAttributes.KindMask)
                            | (value ? PointerAttributes.Far32 : 0);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the pointer is a 64 bit pointer.
    /// </summary>
    public bool IsNear64
    {
        get => (Attributes & PointerAttributes.KindMask) == PointerAttributes.Near64;
        set => Attributes = (Attributes & ~PointerAttributes.KindMask)
                            | (value ? PointerAttributes.Near64 : 0);
    }

    /// <summary>
    /// Gets or sets the mode of the pointer.
    /// </summary>
    public PointerAttributes Mode
    {
        get => Attributes & PointerAttributes.ModeMask;
        set => Attributes = (Attributes & ~PointerAttributes.ModeMask) | (value & PointerAttributes.ModeMask);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the pointer is an "old" reference.
    /// </summary>
    public bool IsLValueReference
    {
        get => (Attributes & PointerAttributes.ModeMask) == PointerAttributes.LValueReference;
        set => Attributes = (Attributes & ~PointerAttributes.ModeMask)
                            | (value ? PointerAttributes.LValueReference : 0);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the pointer is a pointer to data member.
    /// </summary>
    public bool IsPointerToDataMember
    {
        get => (Attributes & PointerAttributes.ModeMask) == PointerAttributes.PointerToDataMember;
        set => Attributes = (Attributes & ~PointerAttributes.ModeMask)
                            | (value ? PointerAttributes.PointerToDataMember : 0);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the pointer is a pointer to member function.
    /// </summary>
    public bool IsPointerToMemberFunction
    {
        get => (Attributes & PointerAttributes.ModeMask) == PointerAttributes.PointerToMemberFunction;
        set => Attributes = (Attributes & ~PointerAttributes.ModeMask)
                            | (value ? PointerAttributes.PointerToMemberFunction : 0);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the pointer is an r-value reference.
    /// </summary>
    public bool IsRValueReference
    {
        get => (Attributes & PointerAttributes.ModeMask) == PointerAttributes.RValueReference;
        set => Attributes = (Attributes & ~PointerAttributes.ModeMask)
                            | (value ? PointerAttributes.RValueReference : 0);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the pointer is a "flat" pointer.
    /// </summary>
    public bool IsFlat32
    {
        get => (Attributes & PointerAttributes.Flat32) != 0;
        set => Attributes = (Attributes & ~PointerAttributes.Flat32)
                            | (value ? PointerAttributes.Flat32 : 0);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the pointer is marked volatile.
    /// </summary>
    public bool IsVolatile
    {
        get => (Attributes & PointerAttributes.Volatile) != 0;
        set => Attributes = (Attributes & ~PointerAttributes.Volatile)
                            | (value ? PointerAttributes.Volatile : 0);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the pointer is marked const.
    /// </summary>
    public bool IsConst
    {
        get => (Attributes & PointerAttributes.Const) != 0;
        set => Attributes = (Attributes & ~PointerAttributes.Const)
                            | (value ? PointerAttributes.Const : 0);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the pointer is marked unaligned.
    /// </summary>
    public bool IsUnaligned
    {
        get => (Attributes & PointerAttributes.Unaligned) != 0;
        set => Attributes = (Attributes & ~PointerAttributes.Unaligned)
                            | (value ? PointerAttributes.Unaligned : 0);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the pointer is marked restrict.
    /// </summary>
    public bool IsRestrict
    {
        get => (Attributes & PointerAttributes.Restrict) != 0;
        set => Attributes = (Attributes & ~PointerAttributes.Restrict)
                            | (value ? PointerAttributes.Restrict : 0);
    }

    /// <summary>
    /// Gets or sets the size of the pointer.
    /// </summary>
    public byte Size
    {
        get => (byte) (((uint) Attributes >> 0xD) & 0b111111);
        set => Attributes = (PointerAttributes) (((uint) Attributes & ~(0b111111u << 0xD))
                                                 | (((uint) value & 0b111111) << 0xD));
    }

    /// <summary>
    /// Gets or sets a value indicating whether the pointer is a WinRT smart pointer.
    /// </summary>
    public bool IsWinRTSmartPointer
    {
        get => (Attributes & PointerAttributes.WinRTSmartPointer) != 0;
        set => Attributes = (Attributes & ~PointerAttributes.WinRTSmartPointer)
                            | (value ? PointerAttributes.WinRTSmartPointer : 0);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the pointer is a 'this' pointer of a member function with ref qualifier.
    /// </summary>
    public bool IsLValueRefThisPointer
    {
        get => (Attributes & PointerAttributes.LValueRefThisPointer) != 0;
        set => Attributes = (Attributes & ~PointerAttributes.LValueRefThisPointer)
                            | (value ? PointerAttributes.LValueRefThisPointer : 0);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the pointer is a 'this' pointer of a member function with ref qualifier.
    /// </summary>
    public bool IsRValueRefThisPointer
    {
        get => (Attributes & PointerAttributes.RValueRefThisPointer) != 0;
        set => Attributes = (Attributes & ~PointerAttributes.RValueRefThisPointer)
                            | (value ? PointerAttributes.RValueRefThisPointer : 0);
    }

    /// <summary>
    /// Obtains the base type of the pointer.
    /// </summary>
    /// <returns>The base type.</returns>
    /// <remarks>
    /// This method is called upon initialization of the <see cref="BaseType"/> property.
    /// </remarks>
    protected virtual CodeViewTypeRecord? GetBaseType() => null;

    /// <inheritdoc />
    public override string ToString() => $"({BaseType})*";
}
