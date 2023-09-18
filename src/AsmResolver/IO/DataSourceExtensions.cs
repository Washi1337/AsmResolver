using System;
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP1_0_OR_GREATER
using System.Buffers;
#endif

namespace AsmResolver.IO
{
    /// <summary>
    /// Provides extension methods for <see cref="IDataSource"/>.
    /// </summary>
    public static class DataSourceExtensions
    {
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
        /// <summary>
        /// Reads a block of data from the data source.
        /// </summary>
        /// <param name="dataSource">The <see cref="IDataSource"/> to read from.</param>
        /// <param name="address">The starting address to read from.</param>
        /// <param name="buffer">The buffer that receives the read bytes.</param>
        /// <returns>The number of bytes that were read.</returns>
        public static int ReadBytes(this IDataSource dataSource, ulong address, Span<byte> buffer)
        {
            if (dataSource is ISpanDataSource spanDataSource)
                return spanDataSource.ReadBytes(address, buffer);

            byte[] array = ArrayPool<byte>.Shared.Rent(buffer.Length);
            int bytesRead = dataSource.ReadBytes(address, array, 0, buffer.Length);
            new ReadOnlySpan<byte>(array, 0, bytesRead).CopyTo(buffer);
            ArrayPool<byte>.Shared.Return(array);
            return bytesRead;
        }
#endif
    }
}
