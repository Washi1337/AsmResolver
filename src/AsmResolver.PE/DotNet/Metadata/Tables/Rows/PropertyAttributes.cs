using System;

namespace AsmResolver.PE.DotNet.Metadata.Tables.Rows
{
    /// <summary>
    /// Provides members defining all flags that can be assigned to a property definition.
    /// </summary>
    [Flags]
    public enum PropertyAttributes : ushort
    {
        /// <summary>
        /// The property uses a special name.
        /// </summary>
        SpecialName = 0x0200,
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