using System;

namespace AsmResolver.IO
{
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
    /// <summary>
    /// Provides span-based methods for writing data to a binary stream.
    /// </summary>
    public interface ISpanBinaryStreamWriter : IBinaryStreamWriter
    {
        /// <summary>
        /// Writes a buffer of data to the stream.
        /// </summary>
        /// <param name="buffer">The buffer to write to the stream.</param>
        void WriteBytes(ReadOnlySpan<byte> buffer);
    }
#endif
}
