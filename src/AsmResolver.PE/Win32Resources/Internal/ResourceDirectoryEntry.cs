using System.Text;
using AsmResolver.PE.File;
using AsmResolver.PE.File.Headers;

namespace AsmResolver.PE.Win32Resources.Internal
{
    internal class ResourceDirectoryEntry
    {
        public const int EntrySize = 2 * sizeof(uint);

        private readonly uint _dataOrSubDirOffset;
        
        public ResourceDirectoryEntry(PEFile peFile, IBinaryStreamReader reader, bool isByName)
        {
            IdOrNameOffset = reader.ReadUInt32();
            _dataOrSubDirOffset = reader.ReadUInt32();

            if (isByName)
            {
                uint baseRva = peFile.OptionalHeader
                    .DataDirectories[OptionalHeader.ResourceDirectoryIndex]
                    .VirtualAddress;
                var nameReader = peFile.CreateReaderAtRva(baseRva + IdOrNameOffset);

                int length = nameReader.ReadUInt16();
                var data = new byte[length];
                length = nameReader.ReadBytes(data, 0, length);

                Name = Encoding.Unicode.GetString(data, 0, length);
            }
        }

        public string Name
        {
            get;
        }

        public uint IdOrNameOffset
        {
            get;
        }

        public bool IsByName => Name != null;

        public uint DataOrSubDirOffset => _dataOrSubDirOffset & 0x7FFFFFFF;

        public bool IsData => (_dataOrSubDirOffset & 0x80000000) == 0;
        
        public bool IsSubDirectory => (_dataOrSubDirOffset & 0x80000000) != 0;
    }
}