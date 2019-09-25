using System;
using System.Diagnostics;
using AsmResolver.Lazy;
using AsmResolver.PE.File;
using AsmResolver.PE.File.Headers;

namespace AsmResolver.PE.Win32Resources.Internal
{
    [DebuggerDisplay("Count = {" + nameof(Count) + "}")]
    internal class ResourceDirectoryEntryList : LazyList<IResourceDirectoryEntry>
    {
        private readonly PEFile _peFile;
        private readonly IBinaryStreamReader _reader;
        private readonly int _namedEntries;
        private readonly int _idEntries;

        public ResourceDirectoryEntryList(PEFile peFile, IBinaryStreamReader reader, int namedEntries, int idEntries)
        {
            _reader = reader ?? throw new ArgumentNullException(nameof(reader));
            _namedEntries = namedEntries;
            _idEntries = idEntries;
            _peFile = peFile;
        }

        protected override void Initialize()
        {
            uint baseRva = _peFile.OptionalHeader.DataDirectories[OptionalHeader.ResourceDirectoryIndex].VirtualAddress;
            
            for (int i = 0; i < _namedEntries + _idEntries; i++)
            {
                var rawEntry = new ResourceDirectoryEntry(_peFile, _reader, i < _namedEntries);
                var entryReader = _peFile.CreateReaderAtRva(baseRva + rawEntry.DataOrSubDirOffset);

                var entry = rawEntry.IsSubDirectory
                    ? (IResourceDirectoryEntry) new ResourceDirectoryInternal(_peFile, rawEntry, entryReader)
                    : new ResourceDataInternal(_peFile, rawEntry, entryReader);
                    
                Items.Add(entry);
            }
        }
        
    }
}