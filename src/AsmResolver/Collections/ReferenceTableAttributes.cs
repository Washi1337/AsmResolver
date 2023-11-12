using System;

namespace AsmResolver.Collections
{
    /// <summary>
    /// Provides members describing the shape of a reference table.
    /// </summary>
    [Flags]
    public enum ReferenceTableAttributes
    {
        /// <summary>
        /// Indicates the table contains offsets.
        /// </summary>
        Offset = 0b00,

        /// <summary>
        /// Indicates the table contains RVAs.
        /// </summary>
        Rva = 0b01,

        /// <summary>
        /// Indicates the table contains VAs.
        /// </summary>
        Va = 0b10,

        /// <summary>
        /// Provides a bit-mask for obtaining the type of references stored in the table.
        /// </summary>
        ReferenceTypeMask = 0b11,

        /// <summary>
        /// Indicates the table changes in size depending on whether it is put in a 32-bit or 64-bit application.
        /// </summary>
        Adaptive = 0b0000,

        /// <summary>
        /// Indicates the table always uses 32 bits for every entry.
        /// </summary>
        Force32Bit = 0b0100,

        /// <summary>
        /// Indicates the table always uses 64 bits for every entry.
        /// </summary>
        Force64Bit = 0b1000,

        /// <summary>
        /// Provides a bit-mask for obtaining the size of each reference stored in the table.
        /// </summary>
        SizeMask = 0b1100,

        /// <summary>
        /// Indicates the table ends with an extra zero item.
        /// </summary>
        ZeroTerminated = 0b10000
    }
}
