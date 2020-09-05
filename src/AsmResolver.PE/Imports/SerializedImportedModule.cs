using System.Collections.Generic;
using AsmResolver.PE.File;
using AsmResolver.PE.File.Headers;

namespace AsmResolver.PE.Imports
{
    /// <summary>
    /// Provides an implementation of a module import entry present in a PE file.
    /// </summary>
    public class SerializedImportedModule : ImportedModule
    {
        /// <summary>
        /// The amount of bytes a single entry uses in the import directory table.
        /// </summary>
        public const uint ModuleImportSize = 5 * sizeof(uint);
        
        private readonly IPEFile _peFile;
        private readonly uint _lookupRva;
        private readonly uint _addressRva;
        
        /// <summary>
        /// Reads a module import entry from an input stream.
        /// </summary>
        /// <param name="peFile">The PE file containing the module import.</param>
        /// <param name="reader">The input stream.</param>
        public SerializedImportedModule(IPEFile peFile, IBinaryStreamReader reader)
        {
            _peFile = peFile;
            _lookupRva = reader.ReadUInt32();
            TimeDateStamp = reader.ReadUInt32();
            ForwarderChain = reader.ReadUInt32();
            uint nameRva = reader.ReadUInt32();
            if (nameRva != 0)
            {
                if (_peFile.TryCreateReaderAtRva(nameRva, out var nameReader))
                    Name = nameReader.ReadAsciiString();
            }

            _addressRva = reader.ReadUInt32();
        }

        /// <summary>
        /// Determines whether the module import is empty, that is whether every field is 0.
        /// </summary>
        /// <remarks>
        /// The PE file format uses an empty module import entry to indicate the end of the list of imported modules.
        /// </remarks>
        public bool IsEmpty =>
            _lookupRva == 0
            && TimeDateStamp == 0
            && ForwarderChain == 0
            && Name == null
            && _addressRva == 0;

        /// <inheritdoc />
        protected override IList<ImportedSymbol> GetSymbols()
        {
            var result = new List<ImportedSymbol>();
            
            if (IsEmpty)
                return result;

            ulong ordinalMask = _peFile.OptionalHeader.Magic == OptionalHeaderMagic.Pe32
                ? 0x8000_0000ul
                : 0x8000_0000_0000_0000ul;
                
            var lookupItems = ReadEntries(_lookupRva);
            var addresses = ReadEntries(_addressRva);

            for (int i = 0; i < lookupItems.Count; i++)
            {
                ImportedSymbol entry;
                
                ulong lookupItem = lookupItems[i];
                if ((lookupItem & ordinalMask) != 0)
                {
                    entry = new ImportedSymbol((ushort) (lookupItem & 0xFFFF));
                }
                else
                {
                    uint hintNameRva = (uint) (lookupItem & 0xFFFFFFFF);
                    var reader = _peFile.CreateReaderAtRva(hintNameRva);
                    entry = new ImportedSymbol(reader.ReadUInt16(), reader.ReadAsciiString());
                }

                entry.Address = addresses[i];

                result.Add(entry);
            }

            return result;
        }
        
        private IList<ulong> ReadEntries(uint rva)
        {
            var result = new List<ulong>();
            if (!_peFile.TryCreateReaderAtRva(rva, out var itemReader))
                return result;

            while (true)
            {
                ulong currentItem = itemReader.ReadNativeInt(_peFile.OptionalHeader.Magic == OptionalHeaderMagic.Pe32);
                if (currentItem == 0)
                    break;
                result.Add(currentItem);
            }

            return result;
        }

    }
}