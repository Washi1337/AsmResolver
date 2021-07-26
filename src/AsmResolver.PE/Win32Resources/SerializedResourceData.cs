using System;
using AsmResolver.IO;

namespace AsmResolver.PE.Win32Resources
{
    /// <summary>
    /// Provides an implementation for a single data entry in a Win32 resource directory, that was read from an existing
    /// PE file.
    /// </summary>
    public class SerializedResourceData : ResourceData
    {
        /// <summary>
        /// Indicates the size of a single data entry in a resource directory.
        /// </summary>
        public const uint ResourceDataEntrySize = 4 * sizeof(uint);

        private readonly PEReaderContext _context;
        private readonly uint _contentsRva;
        private readonly uint _contentsSize;

        /// <summary>
        /// Reads a resource data entry from the provided input stream.
        /// </summary>
        /// <param name="context">The PE reader context.</param>
        /// <param name="entry">The entry to read.</param>
        /// <param name="entryReader">The input stream to read the data from.</param>
        public SerializedResourceData(PEReaderContext context, ResourceDirectoryEntry entry, ref BinaryStreamReader entryReader)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));

            if (entry.IsByName)
                Name = entry.Name;
            else
                Id = entry.IdOrNameOffset;

            _contentsRva = entryReader.ReadUInt32();
            _contentsSize = entryReader.ReadUInt32();
            CodePage = entryReader.ReadUInt32();
        }

        /// <inheritdoc />
        protected override ISegment? GetContents()
        {
            if (!_context.File.TryCreateReaderAtRva(_contentsRva, _contentsSize, out var reader))
            {
                _context.BadImage("Resource data entry contains an invalid RVA and/or size.");
                return null;
            }

            return DataSegment.FromReader(ref reader);
        }

    }
}
