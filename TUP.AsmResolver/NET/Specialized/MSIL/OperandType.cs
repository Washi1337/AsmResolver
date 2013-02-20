using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver.NET.Specialized.MSIL
{
    public enum OperandType
    {
        None,
        String,
        Method,
        Field,
        Type,
        Token,
        InstructionTarget,
        ShortInstructionTarget,
        InstructionTable,
        Int8,
        Int32,
        Int64,
        Float32,
        Float64,
        Variable,
        ShortVariable,
        Argument,
        ShortArgument,
        Signature,
        Phi,



    }
}
