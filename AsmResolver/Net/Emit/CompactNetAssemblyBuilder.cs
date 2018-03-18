using System;
using System.Linq;
using AsmResolver.Emit;

namespace AsmResolver.Net.Emit
{
    public class CompactNetAssemblyBuilder : WindowsAssemblyBuilder
    {
        private ImageSectionHeader _textSectionHeader;
        private ImageSectionHeader _relocSectionHeader;
        private CompactNetTextContents _textContents;
        private ImageSectionHeader _rsrcSectionHeader;

        public CompactNetAssemblyBuilder(WindowsAssembly assembly)
            : base(assembly)
        {
        }

        protected override void CreateSections(EmitContext context)
        {
            CreateTextSection();

            if (Assembly.RootResourceDirectory != null)
                CreateResourceSection();

            if (Assembly.RelocationDirectory != null)
                CreateRelocationSection();

            Assembly.SectionHeaders.Clear();
            Assembly.SectionHeaders.Add(_textSectionHeader);

            if (_relocSectionHeader != null)
                Assembly.SectionHeaders.Add(_relocSectionHeader);
            if (_rsrcSectionHeader != null)
                Assembly.SectionHeaders.Add(_rsrcSectionHeader);
            
            foreach (var section in Assembly.GetSections())
                SectionTable.AddSection(section);
        }

        private void CreateTextSection()
        {
            _textContents = new CompactNetTextContents(Assembly);
            _textSectionHeader = new ImageSectionHeader
            {
                Name = ".text",
                Attributes = ImageSectionAttributes.MemoryExecute |
                             ImageSectionAttributes.MemoryRead |
                             ImageSectionAttributes.ContentCode,
                Section = {Segments = {_textContents}}
            };
        }

        private void CreateRelocationSection()
        {
            Assembly.RelocationDirectory.Blocks.Clear();
            var block = new BaseRelocationBlock(0);
            block.Entries.Add(new BaseRelocationEntry(BaseRelocationType.HighLow, 0));
            block.Entries.Add(new BaseRelocationEntry(BaseRelocationType.Absolute, 0));
            Assembly.RelocationDirectory.Blocks.Add(block);

            _relocSectionHeader = new ImageSectionHeader
            {
                Name = ".reloc",
                Attributes = ImageSectionAttributes.MemoryRead |
                             ImageSectionAttributes.ContentInitializedData,
                Section = {Segments = {Assembly.RelocationDirectory}}
            };
        }

        private void CreateResourceSection()
        {
            var resourcesBuffer = new ResourceDirectoryBuffer(Assembly);
            _rsrcSectionHeader = new ImageSectionHeader
            {
                Name = ".rsrc",
                Attributes = ImageSectionAttributes.MemoryRead |
                             ImageSectionAttributes.ContentInitializedData,
                Section =
                {
                    Segments = {resourcesBuffer.DirectoryTable, resourcesBuffer.DataDirectoryTable, resourcesBuffer.DataTable}
                }
            };
        }

        public override void UpdateReferences(EmitContext context)
        {
            base.UpdateReferences(context);
            UpdateNtHeaders();
            UpdateRelocations();
        }

        private void UpdateNtHeaders()
        {
            UpdateFileHeader();
            UpdateOptionalHeader();
        }

        private void UpdateFileHeader()
        {
            var header = Assembly.NtHeaders.FileHeader;
            header.NumberOfSections = (ushort)SectionTable.GetSections().Count();
            header.SizeOfOptionalHeader = 0xE0;
        }

        private void UpdateOptionalHeader()
        {
            var header = Assembly.NtHeaders.OptionalHeader;
            
            header.SizeOfCode = _textSectionHeader.Section.GetPhysicalLength();
            header.SizeOfInitializedData = (_relocSectionHeader != null ? _relocSectionHeader.Section.GetPhysicalLength() : 0) +
                                           (_rsrcSectionHeader != null ? _rsrcSectionHeader.Section.GetPhysicalLength() : 0);
            header.BaseOfCode = _textSectionHeader.VirtualAddress;
            if (_relocSectionHeader != null)
                header.BaseOfData = _relocSectionHeader.VirtualAddress;

            var lastSection = SectionTable.GetSections().Last().Header;
            header.SizeOfImage = lastSection.VirtualAddress +
                                 Align(lastSection.VirtualSize, Assembly.NtHeaders.OptionalHeader.SectionAlignment);
            header.SizeOfHeaders = 0x200;

            header.AddressOfEntrypoint = (uint) Assembly.FileOffsetToRva(_textContents.Bootstrapper.StartOffset);
            
            UpdateDataDirectories();
        }
        
        private void UpdateDataDirectories()
        {
            var optionalHeader = Assembly.NtHeaders.OptionalHeader;
            
            var relocDirectory = optionalHeader.DataDirectories[ImageDataDirectory.BaseRelocationDirectoryIndex];
            if (_relocSectionHeader == null)
            {
                relocDirectory.VirtualAddress = 0;
                relocDirectory.Size = 0;
            }
            else
            {
                relocDirectory.VirtualAddress = _relocSectionHeader.VirtualAddress;
                relocDirectory.Size = _relocSectionHeader.VirtualSize;
            }
            
            var resourceDirectory = optionalHeader.DataDirectories[ImageDataDirectory.ResourceDirectoryIndex];
            if (_rsrcSectionHeader == null)
            {
                resourceDirectory.VirtualAddress = 0;
                resourceDirectory.Size = 0;
            }
            else
            {
                resourceDirectory.VirtualAddress = _rsrcSectionHeader.VirtualAddress;
                resourceDirectory.Size = _rsrcSectionHeader.VirtualSize;
            }
            
            var importDirectory = optionalHeader.DataDirectories[ImageDataDirectory.ImportDirectoryIndex];
            importDirectory.VirtualAddress = (uint)Assembly.FileOffsetToRva(_textContents.ImportDirectory.StartOffset);
            importDirectory.Size = _textContents.ImportDirectory.GetPhysicalLength();

            var iatDirectory = optionalHeader.DataDirectories[ImageDataDirectory.IatDirectoryIndex];
            iatDirectory.VirtualAddress = (uint) Assembly.FileOffsetToRva(_textContents.ImportBuffer.AddressTables.StartOffset);
            iatDirectory.Size = _textContents.ImportBuffer.AddressTables.GetPhysicalLength();
            
            var debugDirectory = optionalHeader.DataDirectories[ImageDataDirectory.DebugDirectoryIndex];
            if (_textContents.DebugDirectory != null)
            {
                debugDirectory.VirtualAddress = (uint) Assembly.FileOffsetToRva(_textContents.DebugDirectory.StartOffset);
                debugDirectory.Size = _textContents.DebugDirectory.GetPhysicalLength();
            }

            var clrDirectory = optionalHeader.DataDirectories[ImageDataDirectory.ClrDirectoryIndex];
            clrDirectory.VirtualAddress = (uint)Assembly.FileOffsetToRva(_textContents.NetDirectory.StartOffset);
            clrDirectory.Size = _textContents.NetDirectory.GetPhysicalLength();
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
    }
}