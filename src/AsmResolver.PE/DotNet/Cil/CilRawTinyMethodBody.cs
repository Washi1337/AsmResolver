using System;

namespace AsmResolver.PE.DotNet.Cil
{
    /// <summary>
    /// Represents a CIL method body using the tiny format.
    /// </summary>
    /// <remarks>
    /// The tiny method body format is used when a CIL method body's code size is less than 64 bytes, has no local
    /// variables, its max stack size is less than or equal to 8, and has no extra sections (e.g. no exception handlers).
    ///  
    /// This class does not do any encoding/decoding of the bytes that make up the actual CIL instructions, nor does
    /// it do any verification of the code.
    /// </remarks>
    public class CilRawTinyMethodBody : CilRawMethodBody
    {
        /// <summary>
        /// Reads a raw method body from the given binary input stream using the tiny method body format.
        /// </summary>
        /// <param name="reader">The binary input stream to read from.</param>
        /// <returns>The raw method body.</returns>
        /// <exception cref="FormatException">Occurs when the method header indicates an method body that is not in the
        /// tiny format.</exception>
        public new static CilRawTinyMethodBody FromReader(IBinaryStreamReader reader)
        {
            uint fileOffset = reader.FileOffset;
            uint rva = reader.Rva;
            
            var flag = (CilMethodBodyAttributes) reader.ReadByte();
            if ((flag & CilMethodBodyAttributes.Tiny) != CilMethodBodyAttributes.Tiny)
                throw new FormatException("Invalid tiny CIL method body header.");

            int codeSize = (byte) flag >> 2;
            var code = new byte[codeSize];
            reader.ReadBytes(code, 0, codeSize);

            var methodBody = new CilRawTinyMethodBody(code);
            methodBody.UpdateOffsets(fileOffset, rva);
            return methodBody;
        }

        /// <summary>
        /// Creates a new method body using a buffer of assembled CIL instructions.
        /// </summary>
        /// <param name="code">The buffer containing the raw CIL instructions.</param>
        public CilRawTinyMethodBody(byte[] code)
        {
            if (code == null)
                throw new ArgumentNullException(nameof(code));
            
            Code = new byte[code.Length];
            Buffer.BlockCopy(code, 0, Code, 0, code.Length);
        }

        /// <inheritdoc />
        public override bool IsFat => false;

        /// <inheritdoc />
        public override uint GetPhysicalSize() => (uint) (sizeof(byte) + Code.Length);

        /// <inheritdoc />
        public override void Write(IBinaryStreamWriter writer)
        {
            if (Code.Length > 0x3F)
                throw new ArgumentException("Code of a tiny method body cannot be 64 bytes or larger.");
                
            byte flag = (byte) ((byte) CilMethodBodyAttributes.Tiny | (Code.Length << 2));
            writer.WriteByte(flag);
            writer.WriteBytes(Code);
        }
        
    }
}