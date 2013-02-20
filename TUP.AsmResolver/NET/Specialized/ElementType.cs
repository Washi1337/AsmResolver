using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver.NET.Specialized
{
    public enum ElementType : byte
    {    
        Array = 20,
        Boolean = 2,
        Boxed = 81,
        ByRef = 16,
        Char = 3,
        Class = 18,
        CModOpt = 32,
        CModReqD = 31,
        Enum = 85,
        FnPtr = 27,
        GenericInst = 21,
        I = 24,
        I1 = 4,
        I2 = 6,
        I4 = 8,
        I8 = 10,
        Internal = 33,
        Modifier = 64,
        MVar = 30,
        None = 0,
        Object = 28,
        Pinned = 69,
        Ptr = 15,
        R4 = 12,
        R8 = 13,
        Sentinel = 65,
        String = 14,
        SzArray = 29,
        Type = 80,
        TypedByRef = 22,
        U = 25,
        U1 = 5,
        U2 = 7,
        U4 = 9,
        U8 = 11,
        ValueType = 17,
        Var = 19,
        Void = 1

    }
}
