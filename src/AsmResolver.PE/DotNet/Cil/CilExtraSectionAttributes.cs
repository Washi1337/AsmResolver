using System;

namespace AsmResolver.PE.DotNet.Cil
{
    /// <summary>
    /// Defines all possible flags that an extra section in a method body can set.
    /// </summary>
    [Flags]
    public enum CilExtraSectionAttributes : byte
    {
        /// <summary>
        /// Indicates the extra section contains an exception handler table.
        /// </summary>
        EHTable = 0x01,
        
        /// <summary>
        /// Indicates the extra section contains an OptIL table (not supported anymore by the CLR).
        /// </summary>
        OptILTable = 0x02,
        
        /// <summary>
        /// Indicates the extra section uses the fat format to store its data.
        /// </summary>
        FatFormat = 0x40,
        
        /// <summary>
        /// Indicates at least one more section follows this extra section.
        /// </summary>
        MoreSections = 0x80,
    }
}