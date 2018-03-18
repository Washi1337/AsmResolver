using System;
using System.Collections.Generic;
using System.Linq;

namespace AsmResolver.Emit
{
    public class ImportDirectoryBuffer
    {
        public sealed class ModuleImportTableBuffer : FileSegmentBuilder
        {
            public ModuleImportTableBuffer()
            {
                Segments.Add(new ImageModuleImport());
            }

            public void AddModuleImport(ImageModuleImport import)
            {
                Segments.Insert(Segments.Count - 1, import);
            }

            public IEnumerable<ImageModuleImport> GetAddedImports()
            {
                return Segments.OfType<ImageModuleImport>();
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

        public ImportDirectoryBuffer(IOffsetConverter offsetConverter, bool is32Bit)
        {
            Is32Bit = is32Bit;
            if (offsetConverter == null)
                throw new ArgumentNullException("offsetConverter");

            _offsetConverter = offsetConverter;

            ModuleImportTable = new ModuleImportTableBuffer();
            NameTable = new NameTableBuffer();
            LookupTables = new LookupTablesBuffer(_offsetConverter, NameTable);
            AddressTables = new LookupTablesBuffer(_offsetConverter, NameTable);
        }

        public ImportDirectoryBuffer(WindowsAssembly assembly)
            : this(assembly, assembly.NtHeaders.OptionalHeader.Magic == OptionalHeaderMagic.Pe32)
        {
            foreach (var import in assembly.ImportDirectory.ModuleImports)
                AddModuleImport(import);
        }

        public ModuleImportTableBuffer ModuleImportTable
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
            ModuleImportTable.AddModuleImport(moduleImport);
            LookupTables.GetModuleLookupTable(moduleImport);
            AddressTables.GetModuleLookupTable(moduleImport);
            NameTable.GetModuleNameSegment(moduleImport);
        }

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
