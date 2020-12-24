using System.Collections.Generic;
using AsmResolver.PE.Code;

namespace AsmResolver.DotNet.Code.Native
{
    /// <summary>
    /// Represents a method body of a method defined in a .NET assembly, implemented using machine code that runs
    /// natively on the processor. 
    /// </summary>
    public class NativeMethodBody : MethodBody
    {
        /// <summary>
        /// Gets or sets the raw native code stream.
        /// </summary>
        public byte[] Code
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a collection of fixups that need to be applied upon writing the code to the output stream.
        /// This includes addresses to imported symbols and global fields stored in data sections.
        /// </summary>
        public IList<AddressFixup> AddressFixups
        {
            get;
        } = new List<AddressFixup>();
    }
  
}