using System;

namespace AsmResolver.PE.DotNet.Cil
{
    /// <summary>
    /// Provides all possible flags that can be set in the first (two) byte(s) of a CIL method body.
    /// </summary>
    [Flags]
    public enum CilMethodBodyAttributes : ushort
    {
        /// <summary>
        /// Indicates the method body is using the tiny format.
        /// </summary>
        Tiny = 0x2,
        
        /// <summary>
        /// Indicates the method body is using the fat format.
        /// </summary>
        Fat = 0x3,
        
        /// <summary>
        /// Indicates more sections follow after the raw code of the method body.
        /// </summary>
        MoreSections = 0x8,
        
        /// <summary>
        /// Indicates all locals defined in the method body should be initialized to zero by the runtime.
        /// </summary>
        InitLocals = 0x10,
    }
}