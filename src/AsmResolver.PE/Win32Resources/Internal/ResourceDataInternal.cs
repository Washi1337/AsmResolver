using System;
using AsmResolver.PE.File;

namespace AsmResolver.PE.Win32Resources.Internal
{
    internal class ResourceDataInternal : ResourceDataBase
    {
        private readonly PEFile _peFile;
        private readonly ResourceDirectoryEntry _entry;
        private readonly uint _contentsRva;
        private readonly uint _contentsSize;

        public ResourceDataInternal(
            PEFile peFile,
            ResourceDirectoryEntry entry, 
            IBinaryStreamReader reader)
        {
            _peFile = peFile ?? throw new ArgumentNullException(nameof(peFile));
            _entry = entry ?? throw new ArgumentNullException(nameof(entry));

            if (_entry.IsByName)
                Name = entry.Name;
            else
                Id = _entry.IdOrNameOffset;
            
            _contentsRva = reader.ReadUInt32();
            _contentsSize = reader.ReadUInt32();
            CodePage = reader.ReadUInt32();
        }

        protected override IReadableSegment GetContents()
        {
            return DataSegment.FromReader(_peFile.CreateReaderAtRva(_contentsRva, _contentsSize));
        }
        
    }
}