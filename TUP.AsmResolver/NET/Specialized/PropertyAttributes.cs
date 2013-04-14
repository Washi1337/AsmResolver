using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver.NET.Specialized
{
    [Flags]
    public enum PropertyAttributes : ushort
    {
        /// <summary>
        /// The property uses a special name.
        /// </summary>
        SpecialName = 0x0200,

        // Reserved flags for Runtime use only.
        ReservedMask = 0xf400,
        /// <summary>
        /// The runtime should check the name encoding.
        /// </summary>
        RTSpecialName = 0x0400,
        /// <summary>
        /// The proeprty has got a default value.
        /// </summary>
        HasDefault = 0x1000,
    }
}
