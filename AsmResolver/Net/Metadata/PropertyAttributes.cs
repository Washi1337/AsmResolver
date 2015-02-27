using System;

namespace AsmResolver.Net.Metadata
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
        RtSpecialName = 0x0400,
        /// <summary>
        /// The proeprty has got a default value.
        /// </summary>
        HasDefault = 0x1000,
    }
}