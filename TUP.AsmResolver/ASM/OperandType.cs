using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver.ASM
{
    /// <summary>
    /// Value indicating the operand value type.
    /// </summary>
    public enum OperandType
    {
        Normal,
        BytePointer,
        WordPointer,
        DwordPointer,
        FwordPointer,
        QwordPointer,
        LeaRegister,
    }
}
