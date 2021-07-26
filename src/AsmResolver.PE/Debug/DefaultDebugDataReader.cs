using AsmResolver.IO;
using AsmResolver.PE.Debug.CodeView;

namespace AsmResolver.PE.Debug
{
    /// <summary>
    /// Provides a default implementation of the <see cref="IDebugDataReader"/> interface.
    /// </summary>
    public class DefaultDebugDataReader : IDebugDataReader
    {
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
