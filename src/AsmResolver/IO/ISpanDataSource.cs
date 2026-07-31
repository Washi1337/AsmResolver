using System;

namespace AsmResolver.IO
{
    /// <summary>
    /// Provides span-based members for reading data from a data source.
    /// </summary>
    public interface ISpanDataSource : IDataSource
    {
        /// <summary>
        /// Reads a block of data from the data source.
        /// </summary>
        /// <param name="address">The starting address to read from.</param>
        /// <param name="buffer">The buffer that receives the read bytes.</param>
        /// <returns>The number of bytes that were read.</returns>
        int ReadBytes(ulong address, Span<byte> buffer);
    }
}
