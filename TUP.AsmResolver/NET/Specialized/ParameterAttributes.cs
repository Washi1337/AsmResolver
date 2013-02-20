using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver.NET.Specialized
{
    [Flags]
    public enum ParameterAttributes : ushort
    {
        In = 0x0001,     // Param is [In]
        Out = 0x0002,     // Param is [out]
        Optional = 0x0010,     // Param is optional
        
        // Reserved flags for Runtime use only.
        ReservedMask = 0xf000,
        HasDefault = 0x1000,     // Param has default value.
        HasFieldMarshal = 0x2000,     // Param has FieldMarshal.
    }
}
