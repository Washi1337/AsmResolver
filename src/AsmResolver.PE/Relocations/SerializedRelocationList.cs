using AsmResolver.Lazy;
using AsmResolver.PE.File;
using AsmResolver.PE.File.Headers;

namespace AsmResolver.PE.Relocations
{
    public class SerializedRelocationList : LazyList<BaseRelocation>
    {
        private readonly PEFile _peFile;
        private readonly DataDirectory _relocDirectory;

        public SerializedRelocationList(PEFile peFile, DataDirectory relocDirectory)
        {
            _peFile = peFile;
            _relocDirectory = relocDirectory;
        }
        
        protected override void Initialize()
        {
            if (!_peFile.TryCreateDataDirectoryReader(_relocDirectory, out var reader))
                return;

            while (reader.FileOffset < reader.StartPosition + reader.Length) 
                ReadBlock(reader);
        }

        private void ReadBlock(IBinaryStreamReader reader)
        {
            // Read block header. 
            uint pageRva = reader.ReadUInt32();
            uint size = reader.ReadUInt32();

            // Read items.
            int count = (int) ((size - 2 * sizeof(uint)) / sizeof(ushort));
            for (int i = 0; i < count; i++) 
                ReadRelocationEntry(reader, pageRva);
        }

        private void ReadRelocationEntry(IBinaryStreamReader reader, uint pageRva)
        {
            ushort rawValue = reader.ReadUInt16();
            var type = (RelocationType) (rawValue >> 12);
            int offset = rawValue & 0xFFF;

            Items.Add(new BaseRelocation(type, _peFile.GetReferenceToRva((uint) (pageRva + offset))));
        }
    }
}