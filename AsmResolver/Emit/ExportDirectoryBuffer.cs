using System;

namespace AsmResolver.Emit
{
    public class ExportDirectoryBuffer : FileSegmentBuilder
    {
        private readonly IOffsetConverter _converter;
        private readonly ImageExportDirectory _exportDirectory;
        private readonly DataSegment _nameSegment;
        private readonly SimpleFileSegmentBuilder _namesTable;
        private readonly SimpleFileSegmentBuilder _nameRvaTable;
        private readonly SimpleFileSegmentBuilder _nameOrdinalTable;
        private readonly SimpleFileSegmentBuilder _addressesTable;

        public ExportDirectoryBuffer(ImageExportDirectory exportDirectory, IOffsetConverter converter)
        {
            if (exportDirectory == null)
                throw new ArgumentNullException(nameof(exportDirectory));
            _converter = converter;

            Segments.Add(_exportDirectory = exportDirectory);
            Segments.Add(_nameSegment = DataSegment.CreateAsciiString(_exportDirectory.Name, true));
            Segments.Add(_nameRvaTable = new SimpleFileSegmentBuilder());
            Segments.Add(_nameOrdinalTable = new SimpleFileSegmentBuilder());
            Segments.Add(_addressesTable = new SimpleFileSegmentBuilder());
            Segments.Add(_namesTable = new SimpleFileSegmentBuilder());

            foreach (var export in exportDirectory.Exports)
                AddSymbolExport(export);
        }

        public void AddSymbolExport(ImageSymbolExport export)
        {
            if (export.Name != null)
            {
                export.NameOrdinal = (uint) (_addressesTable.Segments.Count + _exportDirectory.OrdinalBase);
                _nameOrdinalTable.Segments.Add(
                    new DataSegment(BitConverter.GetBytes((ushort) _addressesTable.Segments.Count)));
                var nameSegment = DataSegment.CreateAsciiString(export.Name, true);
                _namesTable.Segments.Add(nameSegment);
                _nameRvaTable.Segments.Add(new PointerSegment(nameSegment, _converter, true));
            }
            
            _addressesTable.Segments.Add(DataSegment.CreateNativeInteger(export.Rva, true));

        }

        public override void UpdateReferences(EmitContext context)
        {
            base.UpdateReferences(context);
            _exportDirectory.NameRva = (uint) _converter.FileOffsetToRva(_nameSegment.StartOffset);
            _exportDirectory.NumberOfNames = (uint) _nameRvaTable.Segments.Count;
            _exportDirectory.NumberOfFunctions = (uint) _addressesTable.Segments.Count;
            _exportDirectory.AddressOfNameOrdinals = (uint) _converter.FileOffsetToRva(_nameOrdinalTable.StartOffset);
            _exportDirectory.AddressOfFunctions = (uint) _converter.FileOffsetToRva(_addressesTable.StartOffset);
            _exportDirectory.AddressOfNames = (uint) _converter.FileOffsetToRva(_nameRvaTable.StartOffset);
        }
    }
}