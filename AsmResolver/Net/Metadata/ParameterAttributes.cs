using System;

namespace AsmResolver.Net.Metadata
{
    [Flags]
    public enum ParameterAttributes : ushort
    {
        /// <summary>
        /// Parameter is an input parameter.
        /// </summary>
        In = 0x0001,
        /// <summary>
        /// Parameter is an output parameter.
        /// </summary>
        Out = 0x0002,
        /// <summary>
        /// Parameter is an optional parameter.
        /// </summary>
        Optional = 0x0010,
        // Reserved flags for Runtime use only.
        ReservedMask = 0xf000,
        /// <summary>
        /// Parameter has got a default value.
        /// </summary>
        HasDefault = 0x1000,
        /// <summary>
        /// Parameter has got field marshalling information.
        /// </summary>
        HasFieldMarshal = 0x2000,
    }
}