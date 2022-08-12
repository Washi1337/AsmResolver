using System;

namespace AsmResolver.Symbols.Pdb.Leaves;

/// <summary>
/// Provides members defining all possible attributes that can be assigned to a structure, class or enum type symbol.
/// </summary>
[Flags]
public enum StructureAttributes : ushort
{
    /// <summary>
    /// Indicates the structure is packed.
    /// </summary>
    Packed = 0b00000000_00000001,

    /// <summary>
    /// Indicates the structure defines constructors or destructors.
    /// </summary>
    Ctor = 0b00000000_00000010,

    /// <summary>
    /// Indicates the structure defines overloaded operators.
    /// </summary>
    OvlOps = 0b00000000_00000100,

    /// <summary>
    /// Indicates the structure is a nested class.
    /// </summary>
    IsNested = 0b00000000_00001000,

    /// <summary>
    /// Indicates the structure defines nested types.
    /// </summary>
    CNested = 0b00000000_00010000,

    /// <summary>
    /// Indicates the structure defines an overloaded assignment (=) operator.
    /// </summary>
    OpAssign = 0b00000000_00100000,

    /// <summary>
    /// Indicates the structure defines casting methods.
    /// </summary>
    OpCast = 0b00000000_01000000,

    /// <summary>
    /// Indicates the structure true is a forward reference.
    /// </summary>
    FwdRef = 0b00000000_10000000,

    /// <summary>
    /// Indicates the structure is a scoped definition.
    /// </summary>
    Scoped = 0b00000001_00000000,

    /// <summary>
    /// Indicates the structure has a decorated name following the regular naming conventions.
    /// </summary>
    HasUniqueName = 0b00000010_00000000,

    /// <summary>
    /// Indicates the structure cannot be used as a base class.
    /// </summary>
    Sealed = 0b00000100_00000000,

    /// <summary>
    /// Defines the mask for the floating point type that is used within this structure.
    /// </summary>
    HfaMask = 0b00011000_00000000,

    /// <summary>
    /// Indicates the structure is an intrinsic type.
    /// </summary>
    Intrinsic = 0b00100000_00000000,

    /// <summary>
    /// Defines the mask for the MoCOM type kind.
    /// </summary>
    MoComMask = 0b11000000_00000000,
}
