using AsmResolver.IO;

namespace AsmResolver.PE.Debug.Builder
{
    /// <summary>
    /// Provides a mechanism for building a debug directory in a portable executable (PE) file.
    /// </summary>
    public class DebugDirectoryBuffer : ISegment
    {
        private readonly SegmentBuilder _headers = new();
        private readonly SegmentBuilder _streamsTable = new();

        /// <summary>
        /// Gets the segment buffer that contains all the data streams referenced by the debug directory.
        /// </summary>
        public ISegment ContentsTable => _streamsTable;

        /// <summary>
        /// Gets a value indicating whether the buffer has no entries added to it.
        /// </summary>
        public bool IsEmpty => _headers.Count == 0;

        /// <summary>
        /// Adds a debug data entry to the buffer.
        /// </summary>
        /// <param name="entry">The entry.</param>
        public void AddEntry(DebugDataEntry entry)
        {
            _headers.Add(entry);
            if (entry.Contents is not null and not EmptyDebugDataSegment)
                _streamsTable.Add(entry.Contents, 4);
        }

        /// <inheritdoc />
        public ulong Offset => _headers.Offset;

        /// <inheritdoc />
        public uint Rva => _headers.Rva;

        /// <inheritdoc />
        public bool CanUpdateOffsets => true;

        /// <inheritdoc />
        public void UpdateOffsets(in RelocationParameters parameters) => _headers.UpdateOffsets(parameters);

        /// <inheritdoc />
        public uint GetPhysicalSize() => _headers.GetPhysicalSize();

        /// <inheritdoc />
        public void Write(BinaryStreamWriter writer) => _headers.Write(writer);

        /// <inheritdoc />
        public uint GetVirtualSize() => _headers.GetVirtualSize();
    }
}
