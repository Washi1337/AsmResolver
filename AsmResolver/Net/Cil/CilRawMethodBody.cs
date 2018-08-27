using System;

namespace AsmResolver.Net.Cil
{
    /// <summary>
    /// Provides the base of a raw CIL method body found in a .NET image.
    /// </summary>
    public abstract class CilRawMethodBody : FileSegment
    {
        /// <summary>
        /// Reads a raw method body from the given binary input stream.
        /// </summary>
        /// <param name="reader">The binary input stream to read from.</param>
        /// <returns>The raw method body.</returns>
        /// <exception cref="NotSupportedException">Occurs when the method header indicates an invalid or unsupported
        /// method body format.</exception>
        public static CilRawMethodBody FromReader(IBinaryStreamReader reader)
        {
            byte bodyHeader = reader.ReadByte();
            reader.Position--;
            
            if ((bodyHeader & 0x3) == 0x3)
                return CilRawFatMethodBody.FromReader(reader);
            if ((bodyHeader & 0x2) == 0x2)
                return CilRawSmallMethodBody.FromReader(reader);

            throw new NotSupportedException("Invalid or unsupported method body header.");
        }
        
        /// <summary>
        /// Gets or sets the raw bytes of the CIL code.
        /// </summary>
        public byte[] Code
        {
            get;
            set;
        }
    }
}