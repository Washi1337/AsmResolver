using System.Collections.Generic;
using System.Linq;
using AsmResolver.Builder;

namespace AsmResolver.Net.Builder
{
    public class NetAssemblyBuilder : WindowsAssemblyBuilder
    {
        private sealed class SectionsTableBuilder : FileSegmentBuilder
        {
            private readonly List<SectionBuilder> _sectionBuilders = new List<SectionBuilder>(); 
            private readonly FileSegmentBuilder _headerBuilder = new FileSegmentBuilder();
            private readonly FileSegmentBuilder _contentBuilder = new FileSegmentBuilder();
            private readonly NetAssemblyBuilder _builder;

            public SectionsTableBuilder(NetAssemblyBuilder builder)
            {
                _builder = builder;
                Segments.Add(_headerBuilder);
                Segments.Add(_contentBuilder);
            }

            public SectionBuilder GetSectionBuilder(string sectionName)
            {
                var section = _sectionBuilders.FirstOrDefault(x => x.Header.Name == sectionName);
                if (section == null)
                {
                    _sectionBuilders.Add(section = new SectionBuilder(_builder, sectionName));
                    _headerBuilder.Segments.Add(section.Header);
                    _contentBuilder.Segments.Add(section);
                }
                return section;
            }

            public override void UpdateOffsets(BuildingContext context)
            {
                var fileAlignment = _builder.Assembly.NtHeaders.OptionalHeader.FileAlignment;
                var fileAddress = fileAlignment;

                for (int i = 0; i < _sectionBuilders.Count; i++)
                {
                    if (i == 0)
                        _sectionBuilders[i].Header.StartOffset = StartOffset;
                    else
                        _sectionBuilders[i].Header.StartOffset = _sectionBuilders[i - 1].Header.StartOffset + _sectionBuilders[i - 1].Header.GetPhysicalLength();

                    _sectionBuilders[i].StartOffset = fileAddress;
                    _sectionBuilders[i].UpdateOffsets(context);
                    fileAddress += _sectionBuilders[i].GetPhysicalLength();

                }
            }
            
            public override void UpdateReferences(BuildingContext context)
            {
                var sectionAlignment = _builder.Assembly.NtHeaders.OptionalHeader.SectionAlignment;

                var virtualAddress = sectionAlignment;
                foreach (var section in _sectionBuilders)
                {
                    section.Header.VirtualAddress = virtualAddress;
                    var virtualSize = section.GetVirtualSize();
                    section.Header.VirtualSize = virtualSize;
                    virtualAddress += Align(virtualSize, sectionAlignment);

                    section.Header.PointerToRawData = (uint)section.StartOffset;
                    section.Header.SizeOfRawData = section.GetPhysicalLength();
                }

                base.UpdateReferences(context);
            }
        }

        private readonly SectionsTableBuilder _sectionsTableBuilder;
        private SectionBuilder _textSectionBuilder;
        private SectionBuilder _rsrcSectionBuilder;
        private SectionBuilder _relocSectionBuilder;

        public NetAssemblyBuilder(WindowsAssembly assembly, BuildingParameters parameters)
            : base(assembly, parameters)
        {
            _sectionsTableBuilder = new SectionsTableBuilder(this);
            InitializeBluePrint();
        }

        public NetTextBuilder TextBuilder
        {
            get;
            private set;
        }

        public override void Build(BuildingContext context)
        {
            AppendResourceDirectory();
            AppendRelocationDirectory();
            base.Build(context);
        }

        private void InitializeBluePrint()
        {
            Segments.Add(Assembly.DosHeader);
            Segments.Add(Assembly.NtHeaders);
            Segments.Add(_sectionsTableBuilder);

            _textSectionBuilder = _sectionsTableBuilder.GetSectionBuilder(".text");
            _textSectionBuilder.Header.Attributes = ImageSectionAttributes.MemoryExecute |
                                                    ImageSectionAttributes.MemoryRead |
                                                    ImageSectionAttributes.ContentCode;
            _textSectionBuilder.Segments.Add(TextBuilder = new NetTextBuilder(Assembly.NetDirectory));


            if (Assembly.RootResourceDirectory != null)
            {
                _rsrcSectionBuilder = _sectionsTableBuilder.GetSectionBuilder(".rsrc");
                _rsrcSectionBuilder.Header.Attributes = ImageSectionAttributes.MemoryRead |
                                                        ImageSectionAttributes.ContentInitializedData;
            }

            if (Assembly.RelocationDirectory != null)
            {
                _relocSectionBuilder = _sectionsTableBuilder.GetSectionBuilder(".reloc");
                _relocSectionBuilder.Header.Attributes = ImageSectionAttributes.MemoryRead |
                                                         ImageSectionAttributes.MemoryDiscardable |
                                                         ImageSectionAttributes.ContentInitializedData;
            }
        }


        private void AppendResourceDirectory()
        {
            if (Assembly.RootResourceDirectory != null)
            {
                _rsrcSectionBuilder.Segments.Add(new ResourceDirectoryBuilder(this, _rsrcSectionBuilder.Header,
                    Assembly.RootResourceDirectory));
            }
        }

        private void AppendRelocationDirectory()
        {
            if (Assembly.RelocationDirectory != null)
            {
                Assembly.RelocationDirectory.Blocks.Clear();
                var block = new BaseRelocationBlock(0);
                block.Entries.Add(new BaseRelocationEntry(BaseRelocationType.HighLow, 0));
                block.Entries.Add(new BaseRelocationEntry(BaseRelocationType.Absolute, 0));
                Assembly.RelocationDirectory.Blocks.Add(block);
                _relocSectionBuilder.Segments.Add(Assembly.RelocationDirectory);
            }
        }
        
        public override void UpdateReferences(BuildingContext context)
        {
            UpdateSectionHeaders();
            base.UpdateReferences(context);
            UpdateNtHeaders();
            UpdateRelocations();
        }

        private void UpdateSectionHeaders()
        {
            Assembly.SectionHeaders.Clear();
            Assembly.SectionHeaders.Add(_textSectionBuilder.Header);

            if (_rsrcSectionBuilder != null)
                Assembly.SectionHeaders.Add(_rsrcSectionBuilder.Header);

            if (_relocSectionBuilder != null)
                Assembly.SectionHeaders.Add(_relocSectionBuilder.Header);
        }

        private void UpdateNtHeaders()
        {
            UpdateFileHeader();
            UpdateOptionalHeader();
        }

        private void UpdateFileHeader()
        {
            var header = Assembly.NtHeaders.FileHeader;
            header.NumberOfSections = (ushort)Assembly.SectionHeaders.Count;
            header.SizeOfOptionalHeader = 0xE0;
        }

        private void UpdateOptionalHeader()
        {
            var header = Assembly.NtHeaders.OptionalHeader;
            header.SizeOfCode = _textSectionBuilder.GetPhysicalLength();
            header.SizeOfInitializedData = (_relocSectionBuilder != null ? _relocSectionBuilder.GetPhysicalLength() : 0) +
                                           (_rsrcSectionBuilder != null ? _rsrcSectionBuilder.GetPhysicalLength() : 0);
            header.BaseOfCode = _textSectionBuilder.Header.VirtualAddress;
            if (_relocSectionBuilder != null)
                header.BaseOfData = _relocSectionBuilder.Header.VirtualAddress;

            var lastSection = Assembly.SectionHeaders[Assembly.SectionHeaders.Count - 1];
            header.SizeOfImage = lastSection.VirtualAddress +
                                 Align(lastSection.VirtualSize, Assembly.NtHeaders.OptionalHeader.SectionAlignment);
            header.SizeOfHeaders = 0x200;
            UpdateDataDirectories();
        }

        private void UpdateRelocations()
        {
            if (Assembly.RelocationDirectory != null)
            {
                var block = Assembly.RelocationDirectory.Blocks[0];
                var relocationRva = Assembly.NtHeaders.OptionalHeader.AddressOfEntrypoint + 2;
                block.PageRva = (uint)(relocationRva & ~0xFFF);
                block.Entries[0].Offset = (ushort)(relocationRva - block.PageRva);
            }
        }

        private void UpdateDataDirectories()
        {
            var relocDirectory =
                Assembly.NtHeaders.OptionalHeader.DataDirectories[ImageDataDirectory.BaseRelocationDirectoryIndex];

            if (_relocSectionBuilder == null)
            {
                relocDirectory.VirtualAddress = 0;
                relocDirectory.Size = 0;
            }
            else
            {
                relocDirectory.VirtualAddress = _relocSectionBuilder.Header.VirtualAddress;
                relocDirectory.Size = _relocSectionBuilder.Header.VirtualSize;
            }
            
            var netDirectory =
                Assembly.NtHeaders.OptionalHeader.DataDirectories[ImageDataDirectory.ClrDirectoryIndex];
            netDirectory.VirtualAddress = (uint)Assembly.FileOffsetToRva(Assembly.NetDirectory.StartOffset);
            netDirectory.Size = Assembly.NetDirectory.GetPhysicalLength();
        }
    }
}
