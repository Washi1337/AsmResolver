using System.Text;
using AsmResolver.PE.File;
using AsmResolver.PE.File.Headers;

namespace AsmResolver.PE.Win32Resources
{
    /// <summary>
    /// Represents the raw structure of an entry in a directory.
    /// </summary>
    /// <remarks>
    /// This structure models the IMAGE_DIRECTORY_ENTRY_RESOURCE structure as described in
    /// https://docs.microsoft.com/en-us/windows/win32/debug/pe-format
    /// </remarks>
    public readonly struct ResourceDirectoryEntry
    {
        /// <summary>
        /// The size of a single resource directory entry.
        /// </summary>
        public const int EntrySize = 2 * sizeof(uint);

        private readonly uint _idOrNameOffset;
        private readonly uint _dataOrSubDirOffset;

        /// <summary>
        /// Reads a new resource directory entry from the reader.
        /// </summary>
        /// <param name="peFile">The containing PE file.</param>
        /// <param name="reader">The input stream to read from.</param>
        public ResourceDirectoryEntry(PEFile peFile, IBinaryStreamReader reader)
        {
            _idOrNameOffset = reader.ReadUInt32();
            _dataOrSubDirOffset = reader.ReadUInt32();
            Name = null;

            if (IsByName)
            {
                uint baseRva = peFile.OptionalHeader
                    .DataDirectories[OptionalHeader.ResourceDirectoryIndex]
                    .VirtualAddress;
                
                if (peFile.TryCreateReaderAtRva(baseRva + IdOrNameOffset, out var nameReader))
                {
                    int length = nameReader.ReadUInt16() * 2;
                    var data = new byte[length];
                    length = nameReader.ReadBytes(data, 0, length);

                    Name = Encoding.Unicode.GetString(data, 0, length);
                }
            }
        }

        /// <summary>
        /// Gets the name of the entry (if available).
        /// </summary>
        public string Name
        {
            get;
        }

        /// <summary>
        /// Gets either a 32-integer, or an offset to the name, that identifies the type, name or language ID. 
        /// </summary>
        public uint IdOrNameOffset => _idOrNameOffset & 0x7FFFFFFF;

        /// <summary>
        /// Gets a value indicating whether the resource was exposed by name.
        /// </summary>
        public bool IsByName => (_idOrNameOffset & 0x80000000) != 0;

        /// <summary>
        /// Gets the offset (relative to the beginning of the resource directory) to the contents of the entry.
        /// </summary>
        public uint DataOrSubDirOffset => _dataOrSubDirOffset & 0x7FFFFFFF;

        /// <summary>
        /// Gets a value indicating whether the resource entry is a data entry or not.
        /// </summary>
        public bool IsData => (_dataOrSubDirOffset & 0x80000000) == 0;
        
        /// <summary>
        /// Gets a value indicating whether the resource entry is a sub directory or not.
        /// </summary>
        public bool IsSubDirectory => (_dataOrSubDirOffset & 0x80000000) != 0;

        /// <inheritdoc />
        public override string ToString()
        {
            return $"Entry: {(IsByName ? Name : IdOrNameOffset.ToString())}, Offset: {DataOrSubDirOffset:X8}";
        }
        
    }
}