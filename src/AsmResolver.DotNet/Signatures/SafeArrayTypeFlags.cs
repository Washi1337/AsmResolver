using System;

namespace AsmResolver.DotNet.Signatures
{
    /// <summary>
    /// Provides members defining all valid flags for an element type in a safe array marshal descriptor.  
    /// </summary>
    [Flags]
    public enum SafeArrayTypeFlags
    {
        /// <summary>
        /// Indicates the type is a vector.
        /// </summary>
        Vector = 0x1000,
        
        /// <summary>
        /// Indicates the type is an array.
        /// </summary>
        Array = 0x2000,
        
        /// <summary>
        /// Indicate the type is a by-reference type.
        /// </summary>
        ByRef = 0x4000,
        
        /// <summary>
        /// Provides a bit mask for the type flags. 
        /// </summary>
        Mask = 0xF000,
    }
}