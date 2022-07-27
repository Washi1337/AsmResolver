using System;

namespace AsmResolver.Symbols.Pdb.Leaves;

[Flags]
public enum CodeViewFieldAttributes : ushort
{
    None = 0b00000000_00000000,
    Private = 0b00000000_00000001,
    Protected = 0b00000000_00000010,
    Public = 0b00000000_00000011,
    AccessMask = 0b00000000_00000011,

    MethodPropertiesMask = 0b00000000_00011100,

    Pseudo = 0b00000000_00100000,
    NoInherit = 0b00000000_01000000,
    NoConstruct = 0b00000000_10000000,
    CompilerGenerated = 0b00000001_00000000,
    Sealed = 0b00000010_00000000
}
