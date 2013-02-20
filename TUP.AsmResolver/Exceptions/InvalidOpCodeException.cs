using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver.Exceptions
{
    /// <summary>
    /// Occures when an invalid opcode is detected or tried to be processed.
    /// </summary>
    public class InvalidOpCodeException : Exception
    {        
        /// <summary>
        /// Creates a new instance of the InvalidOpCodeException.
        /// </summary>
        /// <param name="message">The message of the error.</param>
        public InvalidOpCodeException(string message)
            : base(message)
        {
            this.bytes = null;
        }
        /// <summary>
        /// Creates a new instance of the InvalidOpCodeException.
        /// </summary>
        /// <param name="message">The message of the error.</param>
        /// <param name="bytes">The corresponding bytes.</param>
        /// <param name="assembly">The assembly where the error occured.</param>
        public InvalidOpCodeException(string message, byte[] bytes, Win32Assembly assembly)
            : base(message)
        {
            this.bytes = bytes;
            this.assembly = assembly;
        }

        byte[] bytes;
        Win32Assembly assembly;
        /// <summary>
        /// Gets the corresponding bytes.
        /// </summary>
        public byte[] Bytes
        {
            get
            {
                return bytes;
            }
        }
        /// <summary>
        /// Gets the corresponding assembly representation.
        /// </summary>
        public Win32Assembly Assembly
        {
            get
            {
                return assembly;
            }
        }
    }
}
