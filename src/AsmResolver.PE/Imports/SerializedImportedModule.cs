using System;
using System.Collections.Generic;
using AsmResolver.Collections;
using AsmResolver.IO;
using AsmResolver.PE.File;

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

        private readonly PEReaderContext _context;
        private readonly uint _originalFirstThunkRva;
        private readonly uint _firstThunkRva;

        /// <summary>
        /// Reads a module import entry from an input stream.
        /// </summary>
        /// <param name="context">The reader context.</param>
        /// <param name="reader">The input stream.</param>
        public SerializedImportedModule(PEReaderContext context, ref BinaryStreamReader reader)
        {
            if (!reader.IsValid)
                throw new ArgumentNullException(nameof(reader));
            _context = context ?? throw new ArgumentNullException(nameof(context));

            _originalFirstThunkRva = reader.ReadUInt32();
            TimeDateStamp = reader.ReadUInt32();
            ForwarderChain = reader.ReadUInt32();
            uint nameRva = reader.ReadUInt32();
            _firstThunkRva = reader.ReadUInt32();

            if (nameRva != 0)
            {
                if (_context.File.TryCreateReaderAtRva(nameRva, out var nameReader))
                    Name = nameReader.ReadAsciiString();
                else
                    _context.BadImage("Import module contains an invalid name RVA.");
            }
        }

        /// <summary>
        /// Determines whether the module import is empty, that is whether every field is 0.
        /// </summary>
        /// <remarks>
        /// The PE file format uses an empty module import entry to indicate the end of the list of imported modules.
        /// </remarks>
        public bool IsEmpty =>
            _originalFirstThunkRva == 0
            && TimeDateStamp == 0
            && ForwarderChain == 0
            && Name is null
            && _firstThunkRva == 0;

        /// <inheritdoc />
        protected override IList<ImportedSymbol> GetSymbols()
        {
            var result = new OwnedCollection<ImportedModule, ImportedSymbol>(this);

            if (IsEmpty)
                return result;

            bool is32Bit = _context.File.OptionalHeader.Magic == OptionalHeaderMagic.PE32;
            (ulong ordinalMask, int pointerSize) = is32Bit
                ? (0x8000_0000ul, sizeof(uint))
                : (0x8000_0000_0000_0000ul, sizeof(ulong));

            // Prefer OriginalFirstThunk over FirstThunk if it is available and valid.
            if (!_context.File.TryCreateReaderAtRva(_originalFirstThunkRva, out var thunkItemReader)
                && !_context.File.TryCreateReaderAtRva(_firstThunkRva, out thunkItemReader))
            {
                _context.BadImage($"Imported module \"{Name}\" has an invalid import lookup thunk table RVA.");
                return result;
            }

            while (true)
            {
                ImportedSymbol entry;

                // Read next thunk data.
                ulong thunkItem = thunkItemReader.ReadNativeInt(is32Bit);
                if (thunkItem == 0)
                    break;

                // Are we an import by ordinal or by name?
                if ((thunkItem & ordinalMask) != 0)
                {
                    entry = new ImportedSymbol((ushort) (thunkItem & 0xFFFF));
                }
                else
                {
                    // Resolve hint and name.
                    uint hintNameRva = (uint) (thunkItem & 0xFFFFFFFF);
                    if (!_context.File.TryCreateReaderAtRva(hintNameRva, out var reader))
                    {
                        _context.BadImage($"Invalid Hint-Name RVA for import {Name}!#{result.Count.ToString()}.");
                        entry = new ImportedSymbol(0, "<<<INVALID_NAME_RVA>>>");
                    }
                    else
                    {
                        entry = new ImportedSymbol(reader.ReadUInt16(), reader.ReadAsciiString());
                    }
                }

                entry.AddressTableEntry = _context.File.GetReferenceToRva((uint) (_firstThunkRva + result.Count * pointerSize));
                result.Add(entry);
            }

            return result;
        }

    }
}
