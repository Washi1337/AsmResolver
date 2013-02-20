using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver.NET.Specialized
{
    [Flags]
    public enum PropertyAttributes : ushort
    {
        SpecialName = 0x0200,     // property is special. Name describes how.

        // Reserved flags for Runtime use only.
        ReservedMask = 0xf400,
        RTSpecialName = 0x0400,     // Runtime(metadata internal APIs) should check name encoding.
        HasDefault = 0x1000,     // Property has default
    }
}
