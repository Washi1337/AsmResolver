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
        private readonly IPEFile _peFile;
        private readonly uint _nameRva;
        private readonly uint _numberOfFunctions;
        private readonly uint _numberOfNames;
        private readonly uint _addressTableRva;
        private readonly uint _namePointerRva;
        private readonly uint _ordinalTableRva;

        /// <summary>
        /// Reads a single export directory from an input stream.
        /// </summary>
        /// <param name="peFile">The PE file containing the export directory.</param>
        /// <param name="reader">The input stream.</param>
        public SerializedExportDirectory(IPEFile peFile, IBinaryStreamReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));
            _peFile = peFile ?? throw new ArgumentNullException(nameof(peFile));

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
        protected override string GetName() => _peFile.TryCreateReaderAtRva(_nameRva, out var reader)
            ? reader.ReadAsciiString()
            : null;

        /// <inheritdoc />
        protected override IList<ExportedSymbol> GetExports()
        {
            var result = new ExportedSymbolCollection(this);
            
            if (!_peFile.TryCreateReaderAtRva(_addressTableRva, out var addressReader)
                || !_peFile.TryCreateReaderAtRva(_namePointerRva, out var namePointerReader)
                || !_peFile.TryCreateReaderAtRva(_ordinalTableRva, out var ordinalReader))
            {
                return result;
            }

            var ordinalNameTable = ReadOrdinalNameTable(namePointerReader, ordinalReader);

            for (uint i = 0; i < _numberOfFunctions; i++)
            {
                uint rva = addressReader.ReadUInt32();
                ordinalNameTable.TryGetValue(i, out string name);
                result.Add(new ExportedSymbol(_peFile.GetReferenceToRva(rva), name));
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

                if (_peFile.TryCreateReaderAtRva(nameRva, out var nameReader))
                    result[ordinal] = nameReader.ReadAsciiString();
            }

            return result;
        } 
    }
}