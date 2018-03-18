using System;
using System.Collections.Generic;

namespace AsmResolver.Emit
{
    public class ImageImportDirectoryBuffer
    {
        public sealed class ImportTableBuffer : FileSegmentBuilder
        {
            public ImportTableBuffer()
            {
                Segments.Add(new ImageModuleImport());
            }

            public void AddModuleImport(ImageModuleImport import)
            {
                Segments.Insert(Segments.Count - 2, import);
            }
        }

        public sealed class NameTableBuffer : FileSegmentBuilder
        {
            private readonly IDictionary<ImageModuleImport, FileSegment> _moduleNameSegments = new Dictionary<ImageModuleImport, FileSegment>();

            public void AddHintNameSegment(HintName hintName)
            {
                if (!Segments.Contains(hintName))
                    Segments.Add(hintName);
            }

            public FileSegment GetModuleNameSegment(ImageModuleImport moduleImport)
            {
                FileSegment segment;
                if (!_moduleNameSegments.TryGetValue(moduleImport, out segment))
                {
                    Segments.Add(segment = DataSegment.CreateAsciiString(moduleImport.Name, true));
                    _moduleNameSegments.Add(moduleImport, segment);
                }
                return segment;
            }
        }

        public sealed class LookupTablesBuffer : FileSegmentBuilder
        {
            private readonly IDictionary<ImageModuleImport, LookupTableBuffer> _lookupTableSegments = new Dictionary<ImageModuleImport, LookupTableBuffer>();
            private readonly IOffsetConverter _offsetConverter;
            private readonly NameTableBuffer _nameTableBuffer;

            public LookupTablesBuffer(IOffsetConverter offsetConverter, NameTableBuffer nameTableBuffer)
            {
                _offsetConverter = offsetConverter;
                _nameTableBuffer = nameTableBuffer;
            }

            public bool Is32Bit
            {
                get;
                set;
            }

            public LookupTableBuffer GetModuleLookupTable(ImageModuleImport moduleImport)
            {
                LookupTableBuffer segment;
                if (!_lookupTableSegments.TryGetValue(moduleImport, out segment))
                {
                    segment = new LookupTableBuffer(_offsetConverter, _nameTableBuffer)
                    {
                        Is32Bit = Is32Bit
                    };

                    foreach (var symbolImport in moduleImport.SymbolImports)
                        segment.GetLookupSegment(symbolImport);
                    segment.GetLookupSegment(new ImageSymbolImport(0));

                    Segments.Add(segment);
                    _lookupTableSegments.Add(moduleImport, segment);
                }
                return segment;
            }
        }

        public sealed class LookupTableBuffer : FileSegmentBuilder
        {
            private readonly Dictionary<ImageSymbolImport, FileSegment> _lookupSegments = new Dictionary<ImageSymbolImport, FileSegment>();
            private readonly IOffsetConverter _offsetConverter;
            private readonly NameTableBuffer _nameTableBuffer;

            public bool Is32Bit
            {
                get;
                set;
            }

            public LookupTableBuffer(IOffsetConverter offsetConverter, NameTableBuffer nameTableBuffer)
            {
                _offsetConverter = offsetConverter;
                _nameTableBuffer = nameTableBuffer;
            }

            public FileSegment GetLookupSegment(ImageSymbolImport symbolImport)
            {
                FileSegment segment;
                if (!_lookupSegments.TryGetValue(symbolImport, out segment))
                {
                    if (symbolImport.IsImportByOrdinal)
                        segment = DataSegment.CreateNativeInteger(symbolImport.Lookup, Is32Bit);
                    else if (symbolImport.HintName != null)
                    {
                        _nameTableBuffer.AddHintNameSegment(symbolImport.HintName);
                        segment = new PointerSegment(symbolImport.HintName, _offsetConverter, Is32Bit);
                    }
                    else
                        segment = DataSegment.CreateNativeInteger(0, Is32Bit);

                    _lookupSegments.Add(symbolImport, segment);
                    Segments.Add(segment);
                }
                return segment;
            }
        }

        private readonly IList<ImageModuleImport> _imports = new List<ImageModuleImport>();

        private readonly IOffsetConverter _offsetConverter;

        public ImageImportDirectoryBuffer(IOffsetConverter offsetConverter, bool is32Bit)
        {
            Is32Bit = is32Bit;
            if (offsetConverter == null)
                throw new ArgumentNullException("offsetConverter");

            _offsetConverter = offsetConverter;

            ImportTable = new ImportTableBuffer();
            NameTable = new NameTableBuffer();
            LookupTables = new LookupTablesBuffer(_offsetConverter, NameTable);
            AddressTables = new LookupTablesBuffer(_offsetConverter, NameTable);
            
            
        }

        public ImportTableBuffer ImportTable
        {
            get;
            private set;
        }

        public NameTableBuffer NameTable
        {
            get;
            private set;
        }

        public LookupTablesBuffer LookupTables
        {
            get;
            private set;
        }

        public LookupTablesBuffer AddressTables
        {
            get;
            private set;
        }

        public bool Is32Bit
        {
            get;
            private set;
        }

        public void AddModuleImport(ImageModuleImport moduleImport)
        {
            _imports.Add(moduleImport);
            ImportTable.AddModuleImport(moduleImport);
            LookupTables.GetModuleLookupTable(moduleImport);
            AddressTables.GetModuleLookupTable(moduleImport);
            NameTable.GetModuleNameSegment(moduleImport);
        }

        //public void UpdateReferences(EmitContext context)
        //{
        //    UpdateTableRvas();
        //    //UpdateDataDirectories(context);

        //}

        //private void UpdateDataDirectories(EmitContext context)
        //{
        //    var optionalHeader = context.Assembly.NtHeaders.OptionalHeader;
        //    var importDirectory = optionalHeader.DataDirectories[ImageDataDirectory.ImportDirectoryIndex];
        //    importDirectory.VirtualAddress = (uint)_offsetConverter.FileOffsetToRva(_importTableBuilder.StartOffset);
        //    importDirectory.Size = this.GetPhysicalLength();

        //    var iatDirectory = optionalHeader.DataDirectories[ImageDataDirectory.IatDirectoryIndex];
        //    iatDirectory.VirtualAddress = (uint)_offsetConverter.FileOffsetToRva(AddressTables.StartOffset);
        //    iatDirectory.Size = AddressTables.GetPhysicalLength();
        //}

        public void UpdateTableRvas()
        {
            foreach (var module in _imports)
            {
                module.NameRva = (uint)_offsetConverter.FileOffsetToRva(NameTable.GetModuleNameSegment(module).StartOffset);
                module.ImportLookupTableRva = (uint)_offsetConverter.FileOffsetToRva(LookupTables.GetModuleLookupTable(module).StartOffset);
                module.ImportAddressTableRva = (uint)_offsetConverter.FileOffsetToRva(AddressTables.GetModuleLookupTable(module).StartOffset);
            }
        }

    }
}
