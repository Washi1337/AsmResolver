using System;

namespace AsmResolver.IO
{
    /// <summary>
    /// Provides members for creating new binary streams.
    /// </summary>
    public interface IBinaryStreamReaderFactory : IDisposable
    {
        /// <summary>
        /// Creates a new binary reader at the provided address.
        /// </summary>
        /// <param name="address">The raw address to start reading from.</param>
        /// <param name="rva">The virtual address (relative to the image base) that is associated to the raw address.</param>
        /// <param name="length">The number of bytes to read.</param>
        /// <returns>The created reader.</returns>
        BinaryStreamReader CreateReader(ulong address, uint rva, uint length);
    }
}
