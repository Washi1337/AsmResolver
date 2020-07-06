using System;

namespace AsmResolver.PE.DotNet.Cil
{
    /// <summary>
    /// When overridden from this class, represents a chunk of CIL code that implements a method body.
    /// </summary>
    public abstract class CilRawMethodBody : SegmentBase
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
            var flag = (CilMethodBodyAttributes) reader.ReadByte();
            reader.FileOffset--;

            if ((flag & CilMethodBodyAttributes.Fat) == CilMethodBodyAttributes.Fat)
                return CilRawFatMethodBody.FromReader(reader);
            if ((flag & CilMethodBodyAttributes.Tiny) == CilMethodBodyAttributes.Tiny)
                return CilRawTinyMethodBody.FromReader(reader);
            
            throw new NotSupportedException("Invalid or unsupported method body format.");
        }

        private byte[] _code;
        
        /// <summary>
        /// Gets a value indicating whether the method body is using the fat format.
        /// </summary>
        public abstract bool IsFat
        {
            get;
        }

        /// <summary>
        /// Gets or sets the raw bytes that make up the CIL code of the method body. 
        /// </summary>
        public byte[] Code
        {
            get => _code;
            set => _code = value ?? throw new ArgumentNullException(nameof(value));
        }
        
    }
}