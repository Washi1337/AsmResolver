using System;
using AsmResolver.PE.File;

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
        
        private readonly PEFile _peFile;
        private readonly uint _contentsRva;
        private readonly uint _contentsSize;

        /// <summary>
        /// Reads a resource data entry from the provided input stream.
        /// </summary>
        /// <param name="peFile">The PE file containing the resource.</param>
        /// <param name="entry">The entry to read.</param>
        /// <param name="entryReader">The input stream to read the data from.</param>
        public SerializedResourceData(PEFile peFile, ResourceDirectoryEntry entry, IBinaryStreamReader entryReader)
        {
            _peFile = peFile ?? throw new ArgumentNullException(nameof(peFile));

            if (entry.IsByName)
                Name = entry.Name;
            else
                Id = entry.IdOrNameOffset;
            
            _contentsRva = entryReader.ReadUInt32();
            _contentsSize = entryReader.ReadUInt32();
            CodePage = entryReader.ReadUInt32();
        }

        /// <inheritdoc />
        protected override ISegment GetContents()
        {
            return _peFile.TryCreateReaderAtRva(_contentsRva, _contentsSize, out var reader)
                ? DataSegment.FromReader(reader)
                : null;
        }
        
    }
}