namespace AsmResolver.PE.Debug.Builder
{
    /// <summary>
    /// Provides a mechanism for building a debug directory in a portable executable (PE) file.
    /// </summary>
    public class DebugDirectoryBuffer : SegmentBase
    {
        private readonly SegmentBuilder _headers = new SegmentBuilder();
        private readonly SegmentBuilder _streamsTable = new SegmentBuilder();

        /// <summary>
        /// Gets the segment buffer that contains all the data streams referenced by the debug directory. 
        /// </summary>
        public ISegment ContentsTable => _streamsTable;

        /// <summary>
        /// Adds a debug data entry to the buffer.
        /// </summary>
        /// <param name="entry">The entry.</param>
        public void AddEntry(DebugDataEntry entry)
        {
            _headers.Add(entry);
            if (entry.Contents != null)
                _streamsTable.Add(entry.Contents);
        }
        
        /// <inheritdoc />
        public override uint GetPhysicalSize() => _headers.GetPhysicalSize();

        /// <inheritdoc />
        public override void Write(IBinaryStreamWriter writer) => _headers.Write(writer);
    }
}