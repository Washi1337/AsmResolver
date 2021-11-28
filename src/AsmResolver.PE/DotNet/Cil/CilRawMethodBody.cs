using System;
using AsmResolver.IO;

namespace AsmResolver.PE.DotNet.Cil
{
    /// <summary>
    /// When overridden from this class, represents a chunk of CIL code that implements a method body.
    /// </summary>
    public abstract class CilRawMethodBody : SegmentBase
    {
        /// <inheritdoc />
        protected CilRawMethodBody(IReadableSegment code)
        {
            Code = code;
        }

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
        public IReadableSegment Code
        {
            get;
            set;
        }

        /// <summary>
        /// Reads a raw method body from the given binary input stream.
        /// </summary>
        /// <param name="reader">The binary input stream to read from.</param>
        /// <returns>The raw method body.</returns>
        /// <exception cref="NotSupportedException">Occurs when the method header indicates an invalid or unsupported
        /// method body format.</exception>
        public static CilRawMethodBody? FromReader(ref BinaryStreamReader reader) =>
            FromReader(ThrowErrorListener.Instance, ref reader);

        /// <summary>
        /// Reads a raw method body from the given binary input stream.
        /// </summary>
        /// <param name="errorListener">The object responsible for recording parser errors.</param>
        /// <param name="reader">The binary input stream to read from.</param>
        /// <returns>The raw method body.</returns>
        /// <exception cref="NotSupportedException">Occurs when the method header indicates an invalid or unsupported
        /// method body format.</exception>
        public static CilRawMethodBody? FromReader(IErrorListener errorListener, ref BinaryStreamReader reader)
        {
            var flag = (CilMethodBodyAttributes) reader.ReadByte();
            reader.Offset--;

            if ((flag & CilMethodBodyAttributes.Fat) == CilMethodBodyAttributes.Fat)
                return CilRawFatMethodBody.FromReader(errorListener, ref reader);
            if ((flag & CilMethodBodyAttributes.Tiny) == CilMethodBodyAttributes.Tiny)
                return CilRawTinyMethodBody.FromReader(errorListener, ref reader);

            throw new NotSupportedException("Invalid or unsupported method body format.");
        }
    }
}
