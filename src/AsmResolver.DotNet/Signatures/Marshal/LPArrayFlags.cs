using System;

namespace AsmResolver.DotNet.Signatures.Marshal
{
    /// <summary>
    /// Provides flags that can be assigned to an instance of a <see cref="LPArrayMarshalDescriptor"/>.
    /// </summary>
    [Flags]
    public enum LPArrayFlags : ushort
    {
        /// <summary>
        /// Indicates the index of the size-parameter is specified.  
        /// </summary>
        SizeParamIndexSpecified = 0x0001,
        
        /// <summary>
        /// Indicates all reserved bits that might be used in the future.
        /// </summary>
        Reserved                = 0xfffe   
    }
}