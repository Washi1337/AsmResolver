using System;

namespace AsmResolver.Net.Cil
{
    [Flags]
    public enum CilExtraSectionAttributes : byte
    {
        ExceptionHandler = 0x1,
        OptILTable = 0x2,
        FatFormat = 0x40,
        HasMoreSections = 0x80
    }
}