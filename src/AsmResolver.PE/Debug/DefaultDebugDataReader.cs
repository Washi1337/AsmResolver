using AsmResolver.PE.Debug.CodeView;

namespace AsmResolver.PE.Debug
{
    /// <summary>
    /// Provides a default implementation of the <see cref="IDebugDataReader"/> interface.
    /// </summary>
    public class DefaultDebugDataReader : IDebugDataReader
    {
        /// <inheritdoc />
        public IDebugDataSegment ReadDebugData(PEReaderContext context, DebugDataType type,
            IBinaryStreamReader reader)
        {
            return type switch
            {
                DebugDataType.CodeView => CodeViewDataSegment.FromReader(context, reader),
                _ => new CustomDebugDataSegment(type, DataSegment.FromReader(reader))
            };
        }
    }
}
