using AsmResolver.IO;

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
        /// <param name="context">The reader context.</param>
        /// <param name="type">The type of data.</param>
        /// <param name="reader">The input stream to read from.</param>
        /// <returns>The debug data, or <c>null</c> if the debug data could not be read.</returns>
        IDebugDataSegment ReadDebugData(PEReaderContext context, DebugDataType type, ref BinaryStreamReader reader);
    }
}
