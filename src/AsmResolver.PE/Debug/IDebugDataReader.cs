namespace AsmResolver.PE.Debug
{
    /// <summary>
    /// Provides members for reading a stream stored in the debug data directory of a PE image.
    /// </summary>
    public interface IDebugDataReader
    {
        /// <summary>
        /// Reads the contents of a single debug data entry.
        /// </summary>
        /// <param name="type">The type of data.</param>
        /// <param name="rva">The starting address of the debug data.</param>
        /// <param name="size">The size of the debug data.</param>
        /// <returns>The debug data, or <c>null</c> if the debug data could not be read.</returns>
        IDebugDataSegment ReadDebugData(DebugDataType type, uint rva, uint size);
    }
}