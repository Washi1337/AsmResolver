using System;

namespace AsmResolver.IO
{
    /// <summary>
    /// Provides members for creating new binary streams.
    /// </summary>
    public interface IBinaryStreamReaderFactory : IDisposable
    {
        /// <summary>
        /// Gets the maximum length a single binary stream reader produced by this factory can have.
        /// </summary>
        uint MaxLength
        {
            get;
        }

        /// <summary>
        /// Creates a new binary reader at the provided address.
        /// </summary>
        /// <param name="address">The raw address to start reading from.</param>
        /// <param name="rva">The virtual address (relative to the image base) that is associated to the raw address.</param>
        /// <param name="length">The number of bytes to read.</param>
        /// <returns>The created reader.</returns>
        BinaryStreamReader CreateReader(ulong address, uint rva, uint length);
    }

    public static class BinaryStreamReaderFactoryExtensions
    {
        public static BinaryStreamReader CreateReader(this IBinaryStreamReaderFactory factory)
            => factory.CreateReader(0, 0, factory.MaxLength);
    }
}
