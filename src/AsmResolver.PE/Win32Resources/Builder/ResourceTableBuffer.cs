using System;
using System.Collections.Generic;

namespace AsmResolver.PE.Win32Resources.Builder
{
    /// <summary>
    /// Provides a base for all table buffer structures in the resource directory of a PE file.
    /// </summary>
    /// <typeparam name="TEntry">The type of entries to store in the table.</typeparam>
    public abstract class ResourceTableBuffer<TEntry> : SegmentBase
    {
        private readonly IDictionary<TEntry, uint> _entryOffsets = new Dictionary<TEntry, uint>();
        private readonly ISegment _parentBuffer;
        private uint _length;

        /// <summary>
        /// Initializes the table buffer.
        /// </summary>
        /// <param name="parentBuffer">The resource directory segment that contains the table.</param>
        protected ResourceTableBuffer(ISegment parentBuffer)
        {
            _parentBuffer = parentBuffer ?? throw new ArgumentNullException(nameof(parentBuffer));
        }

        /// <summary>
        /// Gets the offset to this segment relative to the start of the resource directory.
        /// </summary>
        /// <remarks>
        /// This property should only be used after the table has been relocated to the right location in the PE file.
        /// </remarks>
        public uint RelativeOffset => Rva - _parentBuffer.Rva;
        
        /// <summary>
        /// Gets an ordered collection of entries that are put into the table.
        /// </summary>
        protected IList<TEntry> Entries
        {
            get;
        } = new List<TEntry>();

        /// <summary>
        /// Adds a single entry to the table.
        /// </summary>
        /// <param name="entry">The entry to add.</param>
        public void AddEntry(TEntry entry)
        {
            if (!_entryOffsets.ContainsKey(entry))
            {
                Entries.Add(entry);
                _entryOffsets[entry] = _length;
                _length += GetEntrySize(entry);
            }
        }

        /// <summary>
        /// Determines the size of a single item in the table.
        /// </summary>
        /// <param name="entry">The item to measure.</param>
        /// <returns>The size in bytes.</returns>
        public abstract uint GetEntrySize(TEntry entry);
        
        /// <summary>
        /// Determines the offset of an entry stored in the table, relative to the start of the resource directory.
        /// </summary>
        /// <param name="entry">The item to get the offset for.</param>
        /// <returns>The offset.</returns>
        /// <remarks>
        /// This method should only be used after the table has been relocated to the right location in the PE file.
        /// </remarks>
        public uint GetEntryOffset(TEntry entry) => RelativeOffset + _entryOffsets[entry];

        /// <inheritdoc />
        public override uint GetPhysicalSize() => _length;

        
    }
}