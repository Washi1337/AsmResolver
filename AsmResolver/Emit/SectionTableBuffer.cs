using System.Collections.Generic;
using System.Linq;

namespace AsmResolver.Emit
{
    public class SectionTableBuffer : FileSegmentBuilder
    {
        private readonly SimpleFileSegmentBuilder _sectionHeaderTable = new SimpleFileSegmentBuilder();
        private readonly SimpleFileSegmentBuilder _sectionTable = new SimpleFileSegmentBuilder();

        public SectionTableBuffer(uint fileAlignment, uint sectionAlignment)
            : this(fileAlignment, sectionAlignment, Enumerable.Empty<ImageSection>())
        {
        }

        public SectionTableBuffer(uint fileAlignment, uint sectionAlignment, IEnumerable<ImageSection> sections)
        {
            Segments.Add(_sectionHeaderTable);
            Segments.Add(_sectionTable);
            
            FileAligment = fileAlignment;
            SectionAlignment = sectionAlignment;
            
            foreach (var section in sections)
                AddSection(section);
        }

        public uint FileAligment
        {
            get;
            set;
        }

        public uint SectionAlignment
        {
            get;
            set;
        }

        public IEnumerable<ImageSection> GetSections()
        {
            return _sectionTable.Segments.OfType<ImageSection>();
        }

        public void AddSection(ImageSection section)
        {
            _sectionHeaderTable.Segments.Add(section.Header);
            _sectionTable.Segments.Add(section);
        }
        
        public override void UpdateOffsets(EmitContext context)
        {
            uint fileAddress = FileAligment;

            for (int i = 0; i < _sectionHeaderTable.Segments.Count; i++)
            {
                var currentHeader = (ImageSectionHeader) _sectionHeaderTable.Segments[i];
                var currentData = currentHeader.Section;

                if (i == 0)
                {
                    currentHeader.StartOffset = StartOffset;
                }
                else
                {
                    var previousHeader = _sectionHeaderTable.Segments[i - 1];
                    currentHeader.StartOffset = previousHeader.StartOffset + previousHeader.GetPhysicalLength();
                }

                currentData.StartOffset = fileAddress;
                currentData.UpdateOffsets(context);
                
                fileAddress += currentData.GetPhysicalLength();
            }
        }
        
        public override void UpdateReferences(EmitContext context)
        {
            uint sectionAlignment = SectionAlignment;

            uint virtualAddress = sectionAlignment;
            foreach (var sectionHeader in _sectionHeaderTable.Segments.OfType<ImageSectionHeader>())
            {
                sectionHeader.VirtualAddress = virtualAddress;
                uint virtualSize = sectionHeader.Section.GetVirtualSize();
                sectionHeader.VirtualSize = virtualSize;
                virtualAddress += Align(virtualSize, sectionAlignment);

                sectionHeader.PointerToRawData = (uint) sectionHeader.Section.StartOffset;
                sectionHeader.SizeOfRawData = sectionHeader.Section.GetPhysicalLength();
            }

            base.UpdateReferences(context);
        }
    }
}