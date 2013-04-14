using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver.NET.Specialized
{
    [Flags]
    public enum EventAttributes : ushort
    {
        /// <summary>
        /// Specifies that the event is using a special name.
        /// </summary>
        SpecialName = 0x0200,
        /// <summary>
        /// Specifies that the runtime should check the name encoding.
        /// </summary>
        RTSpecialName = 0x0400,
    }
}
