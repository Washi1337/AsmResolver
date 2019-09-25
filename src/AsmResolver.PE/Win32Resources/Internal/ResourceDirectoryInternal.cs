using System.Collections.Generic;
using AsmResolver.PE.File;

namespace AsmResolver.PE.Win32Resources.Internal
{
    internal class ResourceDirectoryInternal : ResourceDirectoryBase
    {
        private readonly PEFile _peFile;
        private ushort _namedEntries;
        private ushort _idEntries;
        private IBinaryStreamReader _entriesReader;
        private string _name;

        internal ResourceDirectoryInternal(PEFile peFile, ResourceDirectoryEntry entry, IBinaryStreamReader reader)
        {
            _peFile = peFile;

            if (entry != null)
            {
                if (entry.IsByName)
                    Name = entry.Name;
                else
                    Id = entry.IdOrNameOffset;
            }

            Characteristics = reader.ReadUInt32();
            TimeDateStamp = reader.ReadUInt32();
            MajorVersion = reader.ReadUInt16();
            MinorVersion = reader.ReadUInt16();
            
            _namedEntries = reader.ReadUInt16();
            _idEntries = reader.ReadUInt16();
            _entriesReader = reader.Fork();
            
            reader.FileOffset = (uint) (reader.FileOffset + (_namedEntries + _idEntries) * ResourceDirectoryEntry.EntrySize);
        }

        protected override IList<IResourceDirectoryEntry> GetEntries()
        {
            return new ResourceDirectoryEntryList(_peFile, _entriesReader, _namedEntries, _idEntries);
        }
        
    }
}