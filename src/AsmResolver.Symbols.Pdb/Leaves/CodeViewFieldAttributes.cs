using System;

namespace AsmResolver.Symbols.Pdb.Leaves;

/// <summary>
/// Provides members defining all flags that can be assigned to a field, method or class.
/// </summary>
[Flags]
public enum CodeViewFieldAttributes : ushort
{
    /// <summary>
    /// Indicates no attributes were assigned to the field.
    /// </summary>
    None = 0b00000000_00000000,

    /// <summary>
    /// Indicates the field is marked private.
    /// </summary>
    Private = 0b00000000_00000001,

    /// <summary>
    /// Indicates the field is marked protected.
    /// </summary>
    Protected = 0b00000000_00000010,

    /// <summary>
    /// Indicates the field is marked public.
    /// </summary>
    Public = 0b00000000_00000011,

    /// <summary>
    /// Provides the bit-mask that can be used to extract the access-level of the field.
    /// </summary>
    AccessMask = 0b00000000_00000011,

    /// <summary>
    /// Provides the bit-mask that can be used to extract the method properties of the field.
    /// </summary>
    MethodPropertiesMask = 0b00000000_00011100,

    /// <summary>
    /// Indicates the field is compiler generated and does not exist.
    /// </summary>
    Pseudo = 0b00000000_00100000,

    /// <summary>
    /// Indicates the class cannot be inherited.
    /// </summary>
    NoInherit = 0b00000000_01000000,

    /// <summary>
    /// Indicates the class cannot be constructed.
    /// </summary>
    NoConstruct = 0b00000000_10000000,

    /// <summary>
    /// Indicates the field is compiler generated but does exist.
    /// </summary>
    CompilerGenerated = 0b00000001_00000000,

    /// <summary>
    /// Indicates the method cannot be overridden.
    /// </summary>
    Sealed = 0b00000010_00000000
}
