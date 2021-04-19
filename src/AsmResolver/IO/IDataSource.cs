namespace AsmResolver.IO
{
    /// <summary>
    /// Provides members for reading data from a data source.
    /// </summary>
    public interface IDataSource
    {
        /// <summary>
        /// Reads a single byte at the provided address.
        /// </summary>
        /// <param name="address">The address to read from.</param>
        byte this[ulong address]
        {
            get;
        }

        /// <summary>
        /// Gets the number of bytes accessible in the data source.
        /// </summary>
        ulong Length
        {
            get;
        }

        /// <summary>
        /// Determines whether the provided address is a valid address in the data source.
        /// </summary>
        /// <param name="address">The address to verify.</param>
        /// <returns><c>true</c> if the address is valid, <c>false</c> otherwise.</returns>
        bool IsValidAddress(ulong address);

        /// <summary>
        /// Reads a block of data from the data source.
        /// </summary>
        /// <param name="address">The starting address to read from.</param>
        /// <param name="buffer">The buffer that receives the read bytes.</param>
        /// <param name="index">The index into the buffer to start writing at.</param>
        /// <param name="count">The number of bytes to read.</param>
        /// <returns>The number of bytes that were read.</returns>
        int ReadBytes(ulong address, byte[] buffer, int index, int count);
    }
}
