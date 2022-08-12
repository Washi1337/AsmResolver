using System;

namespace AsmResolver.Symbols.Pdb.Leaves;

/// <summary>
/// Provides members defining all possible flags that can be assigned to a pointer type in a TPI or IPI stream.
/// </summary>
[Flags]
public enum PointerAttributes : uint
{
    /// <summary>
    /// Indicates the pointer is a 16 bit pointer.
    /// </summary>
    Near16 = 0b0000000000_000_000000_00000_000_00000,

    /// <summary>
    /// Indicates the pointer is a 16:16 far pointer.
    /// </summary>
    Far16 = 0b0000000000_000_000000_00000_000_000001,

    /// <summary>
    /// Indicates the pointer is a 16:16 huge pointer.
    /// </summary>
    Huge16 = 0b0000000000_000_000000_00000_000_000010,

    /// <summary>
    /// Indicates the pointer is a based on segment.
    /// </summary>
    BasedOnSegment = 0b0000000000_000_000000_00000_000_000011,

    /// <summary>
    /// Indicates the pointer is a based on value of base.
    /// </summary>
    BasedOnValue = 0b0000000000_000_000000_00000_000_000100,

    /// <summary>
    /// Indicates the pointer is a based on segment value of base.
    /// </summary>
    BasedOnSegmentValue = 0b0000000000_000_000000_00000_000_000101,

    /// <summary>
    /// Indicates the pointer is a based on address of base.
    /// </summary>
    BasedOnAddress = 0b0000000000_000_000000_00000_000_000110,

    /// <summary>
    /// Indicates the pointer is a based on segment address of base.
    /// </summary>
    BasedOnSegmentAddress = 0b0000000000_000_000000_00000_000_000111,

    /// <summary>
    /// Indicates the pointer is a based on type.
    /// </summary>
    BasedOnType = 0b0000000000_000_000000_00000_000_001000,

    /// <summary>
    /// Indicates the pointer is a based on self.
    /// </summary>
    BasedOnSelf = 0b0000000000_000_000000_00000_000_001001,

    /// <summary>
    /// Indicates the pointer is a 32 bit pointer.
    /// </summary>
    Near32 = 0b0000000000_000_000000_00000_000_001010,

    /// <summary>
    /// Indicates the pointer is a 16:32 pointer.
    /// </summary>
    Far32 = 0b0000000000_000_000000_00000_000_001011,

    /// <summary>
    /// Indicates the pointer is a 64 bit pointer.
    /// </summary>
    Near64 = 0b0000000000_000_000000_00000_000_001100,

    /// <summary>
    /// Provides the bit-mask for extracting the pointer kind from the flags.
    /// </summary>
    KindMask = 0b0000000000_000_000000_00000_000_11111,

    /// <summary>
    /// Indicates the pointer is an "old" reference.
    /// </summary>
    LValueReference = 0b0000000000_000_000000_00000_001_00000,

    /// <summary>
    /// Indicates the pointer is a pointer to data member.
    /// </summary>
    PointerToDataMember = 0b0000000000_000_000000_00000_010_00000,

    /// <summary>
    /// Indicates the pointer is a pointer to member function.
    /// </summary>
    PointerToMemberFunction = 0b0000000000_000_000000_00000_011_00000,

    /// <summary>
    /// Indicates the pointer is an r-value reference.
    /// </summary>
    RValueReference = 0b0000000000_000_000000_00000_100_00000,

    /// <summary>
    /// Provides the bit-mask for extracting the pointer mode from the flags.
    /// </summary>
    ModeMask = 0b0000000000_000_000000_00000_111_00000,

    /// <summary>
    /// Indicates the pointer is a "flat" pointer.
    /// </summary>
    Flat32 = 0b0000000000_000_000000_00001_000_00000,

    /// <summary>
    /// Indicates the pointer is marked volatile.
    /// </summary>
    Volatile = 0b0000000000_000_000000_00010_000_00000,

    /// <summary>
    /// Indicates the pointer is marked const.
    /// </summary>
    Const = 0b0000000000_000_000000_00100_000_00000,

    /// <summary>
    /// Indicates the pointer is marked unaligned.
    /// </summary>
    Unaligned = 0b0000000000_000_000000_01000_000_00000,

    /// <summary>
    /// Indicates the pointer is marked restrict.
    /// </summary>
    Restrict = 0b0000000000_000_000000_10000_000_00000,

    /// <summary>
    /// Indicates the pointer is a WinRT smart pointer.
    /// </summary>
    WinRTSmartPointer = 0b0000000000_001_000000_00000_000_00000,

    /// <summary>
    /// Indicates the pointer is a 'this' pointer of a member function with ref qualifier.
    /// </summary>
    LValueRefThisPointer = 0b0000000000_010_000000_00000_000_00000,

    /// <summary>
    /// Indicates the pointer is a 'this' pointer of a member function with ref qualifier.
    /// </summary>
    RValueRefThisPointer = 0b0000000000_100_000000_00000_000_00000
}
