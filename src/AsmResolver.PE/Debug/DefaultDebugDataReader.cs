namespace AsmResolver.PE.Debug
{
    /// <summary>
    /// Provides a default implementation of the <see cref="IDebugDataReader"/> interface.
    /// </summary>
    public class DefaultDebugDataReader : IDebugDataReader
    {
        private readonly ISegmentReferenceResolver _resolver;

        /// <summary>
        /// Creates a new instance of the <see cref="DefaultDebugDataReader"/> class.
        /// </summary>
        /// <param name="resolver">The reference resolver to use to read from.</param>
        public DefaultDebugDataReader(ISegmentReferenceResolver resolver)
        {
            _resolver = resolver;
        }

        /// <inheritdoc />
        public IDebugDataSegment ReadDebugData(DebugDataType type, uint rva, uint size)
        {
            var reference = _resolver.GetReferenceToRva(rva);
            if (reference is null || !reference.CanRead)
                return null;

            var reader = reference.CreateReader();
            reader.ChangeSize(size);
            
            return new CustomDebugDataSegment(type, DataSegment.FromReader(reader));
        }
    }
}