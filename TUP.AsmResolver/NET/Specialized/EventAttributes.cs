using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver.NET.Specialized
{
    [Flags]
    public enum EventAttributes : ushort
    {
        SpecialName = 0x0200,     // event is special. Name describes how.

        // Reserved flags for Runtime use only.
        ReservedMask = 0x0400,
        RTSpecialName = 0x0400,     // Runtime(metadata internal APIs) should check name encoding.
    }
}
