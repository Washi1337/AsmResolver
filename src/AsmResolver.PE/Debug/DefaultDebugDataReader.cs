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
            if (type == DebugDataType.CodeView)
                return new CodeViewDebugDataSegment(type, reader);

            return new CustomDebugDataSegment(type, DataSegment.FromReader(reader));
        }
    }
}
