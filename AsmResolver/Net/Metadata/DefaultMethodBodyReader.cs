using AsmResolver.Net.Cil;

namespace AsmResolver.Net.Metadata
{
    /// <summary>
    /// Provides a default implementation for the method body reader interface.
    /// </summary>
    public class DefaultMethodBodyReader : IRawMethodBodyReader
    {
        /// <summary>
        /// Gets or sets a value indicating whether invalid method bodies should cause exceptions or not.
        /// </summary>
        public bool ThrowOnInvalidMethodBody
        {
            get;
            set;
        } = true;
        
        /// <inheritdoc />
        public virtual FileSegment ReadMethodBody(
            MetadataRow<FileSegment, MethodImplAttributes, MethodAttributes, uint, uint, uint> row,
            IBinaryStreamReader reader)
        {
            try
            {
                return row.Column2.HasFlag(MethodImplAttributes.IL)
                    ? (FileSegment) CilRawMethodBody.FromReader(reader)
                    : new DataSegment(new byte[0])
                        {StartOffset = reader.Position}; // TODO: add support for native methods?
            }
            catch when (!ThrowOnInvalidMethodBody)
            {
                // Ignore when flag is set.
                return null;
            }
        }
    }
}