using System;

namespace AsmResolver.PE.DotNet.VTableFixup
{
    /// <summary>
    /// Types of VTable entries
    /// </summary>
    [Flags]
    public enum VTableType : ushort
    {
        /// <summary>
        /// VTable slots are 32 bits.
        /// </summary>
        VTable32Bit = 0x01,
        /// <summary>
        /// VTable slots are 64 bits.
        /// </summary>
        VTable64Bit = 0x02,
        /// <summary>
        /// Transition from unmanaged to managed code.
        /// </summary>
        VTableFromUnmanaged = 0x04,
        /// <summary>
        /// Call most derived method described by the token (only valid for virtual methods)
        /// </summary>
        VTableCallMostDerived = 0x10
    }
}
