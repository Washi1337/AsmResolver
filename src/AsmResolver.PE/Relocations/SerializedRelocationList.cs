using System.Diagnostics;
using AsmResolver.Collections;
using AsmResolver.PE.File;
using AsmResolver.PE.File.Headers;

namespace AsmResolver.PE.Relocations
{
    /// <summary>
    /// Provides an implementation of a lazy-initialized list of base relocation entries, read from an existing PE file.
    /// </summary>
    [DebuggerDisplay("Count = {" + nameof(Count) + "}")]
    public class SerializedRelocationList : LazyList<BaseRelocation>
    {
        private readonly IPEFile _peFile;
        private readonly DataDirectory _relocDirectory;

        /// <summary>
        /// Prepares a new lazy-initialized list of base relocations.
        /// </summary>
        /// <param name="peFile">The PE file to read the base relocations from.</param>
        /// <param name="relocDirectory">The directory that contains the base relocations.</param>
        public SerializedRelocationList(IPEFile peFile, DataDirectory relocDirectory)
        {
            _peFile = peFile;
            _relocDirectory = relocDirectory;
        }

        /// <inheritdoc />
        protected override void Initialize()
        {
            if (!_peFile.TryCreateDataDirectoryReader(_relocDirectory, out var reader))
                return;

            while (reader.Offset < reader.StartOffset + reader.Length) 
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