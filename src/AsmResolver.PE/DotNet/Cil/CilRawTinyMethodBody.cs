using System;
using AsmResolver.IO;

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
        /// Creates a new method body using a buffer of assembled CIL instructions.
        /// </summary>
        /// <param name="code">The buffer containing the raw CIL instructions.</param>
        public CilRawTinyMethodBody(byte[] code)
            : base(new DataSegment(code))
        {
        }

        /// <summary>
        /// Creates a new method body using a buffer of assembled CIL instructions.
        /// </summary>
        /// <param name="code">The buffer containing the raw CIL instructions.</param>
        public CilRawTinyMethodBody(IReadableSegment code)
            : base(code)
        {
        }

        /// <inheritdoc />
        public override bool IsFat => false;

        /// <summary>
        /// Reads a raw method body from the given binary input stream using the tiny method body format.
        /// </summary>
        /// <param name="errorListener">The object responsible for recording parser errors.</param>
        /// <param name="reader">The binary input stream to read from.</param>
        /// <returns>The raw method body.</returns>
        /// <exception cref="FormatException">Occurs when the method header indicates an method body that is not in the
        /// tiny format.</exception>
        public new static CilRawTinyMethodBody? FromReader(IErrorListener errorListener, ref BinaryStreamReader reader)
        {
            ulong fileOffset = reader.Offset;
            uint rva = reader.Rva;

            var flag = (CilMethodBodyAttributes) reader.ReadByte();
            if ((flag & CilMethodBodyAttributes.Tiny) != CilMethodBodyAttributes.Tiny)
            {
                errorListener.BadImage("Invalid tiny CIL method body header.");
                return null;
            }

            uint codeSize = (uint) flag >> 2;
            var methodBody = new CilRawTinyMethodBody(reader.ReadSegment(codeSize));
            methodBody.UpdateOffsets(new RelocationParameters(fileOffset, rva));
            return methodBody;
        }

        /// <inheritdoc />
        public override void UpdateOffsets(in RelocationParameters parameters)
        {
            base.UpdateOffsets(parameters);
            Code.UpdateOffsets(parameters.WithAdvance(1));
        }

        /// <inheritdoc />
        public override uint GetPhysicalSize() => sizeof(byte) + Code.GetPhysicalSize();

        /// <inheritdoc />
        public override void Write(BinaryStreamWriter writer)
        {
            uint codeSize = Code.GetPhysicalSize();
            if (codeSize > 0x3F)
                throw new ArgumentException("Code of a tiny method body cannot be 64 bytes or larger.");

            byte flag = (byte) ((byte) CilMethodBodyAttributes.Tiny | (codeSize << 2));
            writer.WriteByte(flag);
            Code?.Write(writer);
        }
    }
}
