using System;

namespace AsmResolver.PE.DotNet.Cil
{
    /// <summary>
    /// Represents a label used to reference an instruction in a CIL method body. 
    /// </summary>
    public interface ICilLabel : IEquatable<ICilLabel>
    {
        /// <summary>
        /// Gets the offset of the referenced instruction.
        /// </summary>
        int Offset
        {
            get;
        }
    }
}