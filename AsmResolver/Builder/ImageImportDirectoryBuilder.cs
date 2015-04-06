using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using AsmResolver.Net.Builder;

namespace AsmResolver.Builder
{
    public class ImageImportDirectoryBuilder : FileSegmentBuilder
    {
        public sealed class NameTableBuilder : FileSegmentBuilder
        {
            private readonly Dictionary<ImageModuleImport, FileSegment> _moduleNameSegments = new Dictionary<ImageModuleImport, FileSegment>();

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

        public sealed class LookupTablesBuilder : FileSegmentBuilder
        {
            private readonly Dictionary<ImageModuleImport, LookupTableBuilder> _lookupTableSegments = new Dictionary<ImageModuleImport, LookupTableBuilder>();
            private readonly IOffsetConverter _offsetConverter;
            private readonly NameTableBuilder _nameTableBuilder;

            public LookupTablesBuilder(IOffsetConverter offsetConverter, NameTableBuilder nameTableBuilder)
            {
                _offsetConverter = offsetConverter;
                _nameTableBuilder = nameTableBuilder;
            }

            public bool Is32Bit
            {
                get;
                set;
            }

            public LookupTableBuilder GetModuleLookupSegment(ImageModuleImport moduleImport)
            {
                LookupTableBuilder segment;
                if (!_lookupTableSegments.TryGetValue(moduleImport, out segment))
                {
                    segment = new LookupTableBuilder(_offsetConverter, _nameTableBuilder)
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

        public sealed class LookupTableBuilder : FileSegmentBuilder
        {
            private readonly Dictionary<ImageSymbolImport, FileSegment> _lookupSegments = new Dictionary<ImageSymbolImport, FileSegment>();
            private readonly IOffsetConverter _offsetConverter;
            private readonly NameTableBuilder _nameTableBuilder;

            public bool Is32Bit
            {
                get;
                set;
            }

            public LookupTableBuilder(IOffsetConverter offsetConverter, NameTableBuilder nameTableBuilder)
            {
                _offsetConverter = offsetConverter;
                _nameTableBuilder = nameTableBuilder;
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
                        _nameTableBuilder.AddHintNameSegment(symbolImport.HintName);
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

        private readonly FileSegmentBuilder _entryTableBuilder;
        private readonly LookupTablesBuilder _lookupTablesBuilder;
        private readonly NameTableBuilder _nameTableBuilder;
        private readonly IOffsetConverter _offsetConverter;
        private readonly ImageImportDirectory _directory;

        public ImageImportDirectoryBuilder(IOffsetConverter offsetConverter, ImageImportDirectory directory)
        {
            if (offsetConverter == null)
                throw new ArgumentNullException("offsetConverter");
            if (directory == null)
                throw new ArgumentNullException("directory");

            _offsetConverter = offsetConverter;
            _directory = directory;
            
            _entryTableBuilder = new FileSegmentBuilder();
            _nameTableBuilder = new NameTableBuilder();
            _lookupTablesBuilder = new LookupTablesBuilder(_offsetConverter, _nameTableBuilder);
            AddressTablesBuilder = new LookupTablesBuilder(_offsetConverter, _nameTableBuilder);

            //Segments.Add(_addressTablesBuilder);
            Segments.Add(_entryTableBuilder);
            Segments.Add(_lookupTablesBuilder);
            Segments.Add(_nameTableBuilder);
        }

        public LookupTablesBuilder AddressTablesBuilder
        {
            get;
            private set;
        }

        public override void Build(BuildingContext context)
        {
            var is32Bit = context.Assembly.NtHeaders.OptionalHeader.Magic == OptionalHeaderMagic.Pe32;
            _lookupTablesBuilder.Is32Bit = is32Bit;
            AddressTablesBuilder.Is32Bit = is32Bit;

            foreach (var moduleImport in _directory.ModuleImports)
                AppendModuleImport(moduleImport);
            _entryTableBuilder.Segments.Add(new ImageModuleImport());
            base.Build(context);
        }

        private void AppendModuleImport(ImageModuleImport moduleImport)
        {
            _entryTableBuilder.Segments.Add(moduleImport);
            _lookupTablesBuilder.GetModuleLookupSegment(moduleImport);
            AddressTablesBuilder.GetModuleLookupSegment(moduleImport);
            _nameTableBuilder.GetModuleNameSegment(moduleImport);
        }

        public override void UpdateReferences(BuildingContext context)
        {
            UpdateTableRvas();
            UpdateDataDirectories(context);
            base.UpdateReferences(context);
        }

        private void UpdateDataDirectories(BuildingContext context)
        {
            var optionalHeader = context.Assembly.NtHeaders.OptionalHeader;
            var importDirectory = optionalHeader.DataDirectories[ImageDataDirectory.ImportDirectoryIndex];
            importDirectory.VirtualAddress = (uint)_offsetConverter.FileOffsetToRva(_entryTableBuilder.StartOffset);
            importDirectory.Size = this.GetPhysicalLength();

            var iatDirectory = optionalHeader.DataDirectories[ImageDataDirectory.IatDirectoryIndex];
            iatDirectory.VirtualAddress = (uint)_offsetConverter.FileOffsetToRva(AddressTablesBuilder.StartOffset);
            iatDirectory.Size = AddressTablesBuilder.GetPhysicalLength();
        }

        private void UpdateTableRvas()
        {
            foreach (var module in _directory.ModuleImports)
            {
                module.NameRva = (uint)_offsetConverter.FileOffsetToRva(_nameTableBuilder.GetModuleNameSegment(module).StartOffset);
                module.ImportLookupTableRva = (uint)_offsetConverter.FileOffsetToRva(_lookupTablesBuilder.GetModuleLookupSegment(module).StartOffset);
                module.ImportAddressTableRva = (uint)_offsetConverter.FileOffsetToRva(AddressTablesBuilder.GetModuleLookupSegment(module).StartOffset);
            }
        }

    }
}
