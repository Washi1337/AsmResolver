using AsmResolver.IO;

namespace AsmResolver.PE.Debug
{
    /// <summary>
    /// Provides a default implementation of the <see cref="IDebugDataReader"/> interface.
    /// </summary>
    public class DefaultDebugDataReader : IDebugDataReader
    {
        /// <summary>
        /// Gets the singleton instance of the <see cref="DefaultDebugDataReader"/> class.
        /// </summary>
        public static DefaultDebugDataReader Instance { get; } = new();

        /// <inheritdoc />
        public IDebugDataSegment? ReadDebugData(PEReaderContext context, DebugDataType type,
            ref BinaryStreamReader reader)
        {
            return type switch
            {
                DebugDataType.CodeView => CodeViewDataSegment.FromReader(context, ref reader),
                _ => new CustomDebugDataSegment(type, DataSegment.FromReader(ref reader))
            };
        }
    }
}
