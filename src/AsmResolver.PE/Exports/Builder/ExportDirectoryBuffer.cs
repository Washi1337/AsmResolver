using System;
using System.Linq;

namespace AsmResolver.PE.Exports.Builder
{
    public class ExportDirectoryBuffer : SegmentBase
    {
        public const uint ExportDirectorySize =
                sizeof(uint) // ExportFlags
                + sizeof(uint) // TimeDateStamp
                + sizeof(ushort) // MajorVersion
                + sizeof(ushort) // MinorVersion
                + sizeof(uint) // NameRVA
                + sizeof(uint) // BaseOrdinal
                + sizeof(uint) // NumberOfFunctions
                + sizeof(uint) // NumberOfNames
                + sizeof(uint) // AddressTableRva
                + sizeof(uint) // NamePointerRVA
                + sizeof(uint) // OrdinalTableRVA
            ;
        
        private readonly SegmentBuilder _contentsBuilder;
        private readonly AddressTableBuffer _addressTableBuffer;
        private readonly OrdinalNamePointerTableBuffer _ordinalNamePointerTable;
        private readonly NameTableBuffer _nameTableBuffer;
        
        private IExportDirectory _exportDirectory;

        public ExportDirectoryBuffer()
        {
            // Initialize table buffers.
            _addressTableBuffer = new AddressTableBuffer();
            _nameTableBuffer = new NameTableBuffer();
            _ordinalNamePointerTable = new OrdinalNamePointerTableBuffer(_nameTableBuffer);

            _contentsBuilder = new SegmentBuilder
            {
                _addressTableBuffer,
                _ordinalNamePointerTable,
                _nameTableBuffer
            };
        }

        public bool IsEmpty => _exportDirectory is null;

        public void AddDirectory(IExportDirectory exportDirectory)
        {
            if (!IsEmpty)
                throw new InvalidProgramException("Cannot add a secondary export directory to the buffer.");
            
            // Set header.
            _exportDirectory = exportDirectory;
            
            // Add contents.
            _nameTableBuffer.AddName(exportDirectory.Name);
            foreach (var symbol in exportDirectory.Entries)
            {
                _addressTableBuffer.AddSymbol(symbol);
                _ordinalNamePointerTable.AddSymbol(symbol);
                if (symbol.IsByName)
                    _nameTableBuffer.AddName(symbol.Name);
            }
        }
        
        /// <inheritdoc />
        public override void UpdateOffsets(uint newFileOffset, uint newRva)
        {
            base.UpdateOffsets(newFileOffset, newRva);
            _contentsBuilder.UpdateOffsets(newFileOffset + ExportDirectorySize, newRva + ExportDirectorySize);
        }

        /// <inheritdoc />
        public override uint GetPhysicalSize() => ExportDirectorySize + _contentsBuilder.GetPhysicalSize();

        /// <inheritdoc />
        public override void Write(IBinaryStreamWriter writer)
        {
            WriteExportDirectoryHeader(writer);
            _contentsBuilder.Write(writer);
        }
        
        private void WriteExportDirectoryHeader(IBinaryStreamWriter writer)
        {
            writer.WriteUInt32(_exportDirectory.ExportFlags);
            writer.WriteUInt32(_exportDirectory.TimeDateStamp);
            writer.WriteUInt16(_exportDirectory.MajorVersion);
            writer.WriteUInt16(_exportDirectory.MinorVersion);
            writer.WriteUInt32(_nameTableBuffer.GetNameRva(_exportDirectory.Name));
            writer.WriteUInt32(_exportDirectory.BaseOrdinal);
            writer.WriteUInt32((uint) _exportDirectory.Entries.Count);
            writer.WriteUInt32((uint) _exportDirectory.Entries.Count(e => e.IsByName));
            writer.WriteUInt32(_addressTableBuffer.Rva);
            writer.WriteUInt32(_ordinalNamePointerTable.NamePointerTableRva);
            writer.WriteUInt32(_ordinalNamePointerTable.OrdinalTableRva);
        }
    }
}