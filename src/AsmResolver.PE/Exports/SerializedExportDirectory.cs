using System;
using System.Collections.Generic;
using AsmResolver.PE.File;

namespace AsmResolver.PE.Exports
{
    /// <summary>
    /// Provides an implementation of an exports directory that was read from an existing PE file.
    /// </summary>
    public class SerializedExportDirectory : ExportDirectory
    {
        private readonly PEReadContext _context;
        private readonly uint _nameRva;
        private readonly uint _numberOfFunctions;
        private readonly uint _numberOfNames;
        private readonly uint _addressTableRva;
        private readonly uint _namePointerRva;
        private readonly uint _ordinalTableRva;

        /// <summary>
        /// Reads a single export directory from an input stream.
        /// </summary>
        /// <param name="context">The reader context.</param>
        /// <param name="reader">The input stream.</param>
        public SerializedExportDirectory(PEReadContext context, IBinaryStreamReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));
            _context = context ?? throw new ArgumentNullException(nameof(context));

            ExportFlags = reader.ReadUInt32();
            TimeDateStamp = reader.ReadUInt32();
            MajorVersion = reader.ReadUInt16();
            MinorVersion = reader.ReadUInt16();
            _nameRva = reader.ReadUInt32();
            BaseOrdinal = reader.ReadUInt32();
            _numberOfFunctions = reader.ReadUInt32();
            _numberOfNames = reader.ReadUInt32();
            _addressTableRva = reader.ReadUInt32();
            _namePointerRva = reader.ReadUInt32();
            _ordinalTableRva = reader.ReadUInt32();
        }

        /// <inheritdoc />
        protected override string GetName()
        {
            if (!_context.File.TryCreateReaderAtRva(_nameRva, out var reader))
            {
                _context.BadImage("Export directory contains an invalid name RVA.");
                return null;
            }
            
            return reader.ReadAsciiString();
        }

        /// <inheritdoc />
        protected override IList<ExportedSymbol> GetExports()
        {
            var result = new ExportedSymbolCollection(this);

            if (!_context.File.TryCreateReaderAtRva(_addressTableRva, out var addressReader))
                _context.BadImage("Export directory contains an invalid address table RVA.");
            
            if (!_context.File.TryCreateReaderAtRva(_namePointerRva, out var namePointerReader))
                _context.BadImage("Export directory contains an invalid name pointer table RVA.");

            if (!_context.File.TryCreateReaderAtRva(_ordinalTableRva, out var ordinalReader))
                _context.BadImage("Export directory contains an invalid ordinal table RVA.");
            
            if (addressReader is null || namePointerReader is null || ordinalReader is null)
                return result;

            var ordinalNameTable = ReadOrdinalNameTable(namePointerReader, ordinalReader);

            for (uint i = 0; i < _numberOfFunctions; i++)
            {
                uint rva = addressReader.ReadUInt32();
                ordinalNameTable.TryGetValue(i, out string name);
                result.Add(new ExportedSymbol(_context.File.GetReferenceToRva(rva), name));
            }

            return result;
        }

        private IDictionary<uint, string> ReadOrdinalNameTable(
            IBinaryStreamReader namePointerReader, IBinaryStreamReader ordinalReader)
        {
            var result = new Dictionary<uint, string>();
            
            for (int i = 0; i < _numberOfNames; i++)
            {
                uint ordinal = ordinalReader.ReadUInt16();
                uint nameRva = namePointerReader.ReadUInt32();

                if (_context.File.TryCreateReaderAtRva(nameRva, out var nameReader))
                    result[ordinal] = nameReader.ReadAsciiString();
            }

            return result;
        } 
    }
}