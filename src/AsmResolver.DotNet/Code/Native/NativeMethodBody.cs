using System;
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
        /// Creates a new empty native method body.
        /// </summary>
        /// <param name="owner">The method that owns the body.</param>
        public NativeMethodBody(MethodDefinition owner)
            : base(owner)
        {
            Code = Array.Empty<byte>();
        }

        /// <summary>
        /// Creates a new native method body with the provided raw code stream.
        /// </summary>
        /// <param name="owner">The method that owns the body.</param>
        /// <param name="code">The raw code stream.</param>
        public NativeMethodBody(MethodDefinition owner, byte[] code)
            : base(owner)
        {
            Code = code;
        }

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
