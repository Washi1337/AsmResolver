namespace AsmResolver.PE.Debug.Builder
{
    /// <summary>
    /// Provides a mechanism for building a debug directory in a portable executable (PE) file.
    /// </summary>
    public class DebugDirectoryBuffer : ISegment
    {
        private readonly SegmentBuilder _headers = new SegmentBuilder();
        private readonly SegmentBuilder _streamsTable = new SegmentBuilder();

        /// <summary>
        /// Gets the segment buffer that contains all the data streams referenced by the debug directory. 
        /// </summary>
        public ISegment ContentsTable => _streamsTable;

        public bool IsEmpty => _headers.Count == 0;

        /// <summary>
        /// Adds a debug data entry to the buffer.
        /// </summary>
        /// <param name="entry">The entry.</param>
        public void AddEntry(DebugDataEntry entry)
        {
            _headers.Add(entry);
            if (entry.Contents != null)
                _streamsTable.Add(entry.Contents, 4);
        }

        /// <inheritdoc />
        public uint FileOffset => _headers.FileOffset;

        /// <inheritdoc />
        public uint Rva => _headers.Rva;

        /// <inheritdoc />
        public bool CanUpdateOffsets => true;

        /// <inheritdoc />
        public void UpdateOffsets(uint newFileOffset, uint newRva) => _headers.UpdateOffsets(newFileOffset, newRva);

        /// <inheritdoc />
        public uint GetPhysicalSize() => _headers.GetPhysicalSize();

        /// <inheritdoc />
        public void Write(IBinaryStreamWriter writer) => _headers.Write(writer);

        /// <inheritdoc />
        public uint GetVirtualSize() => _headers.GetVirtualSize();
    }
}