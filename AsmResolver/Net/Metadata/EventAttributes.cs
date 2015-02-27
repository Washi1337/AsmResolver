using System;

namespace AsmResolver.Net.Metadata
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
        RtSpecialName = 0x0400,
    }
}