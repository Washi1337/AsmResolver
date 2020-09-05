using System;
using System.Collections.Generic;
using AsmResolver.PE.File.Headers;

namespace AsmResolver.PE.File
{
    public class SerializedPEFile : PEFile
    {
        private readonly IList<SectionHeader> _sectionHeaders;
        private readonly IBinaryStreamReader _reader;

        public SerializedPEFile(IBinaryStreamReader reader)
        {
            _reader = reader ?? throw new ArgumentNullException(nameof(reader));
            
            // DOS header.
            DosHeader = DosHeader.FromReader(reader);
            reader.Offset = DosHeader.NextHeaderOffset;

            uint signature = reader.ReadUInt32();
            if (signature != ValidPESignature)
                throw new BadImageFormatException();

            // Read NT headers. 
            FileHeader = FileHeader.FromReader(reader);
            OptionalHeader = OptionalHeader.FromReader(reader);

            // Read section headers.
            reader.Offset = OptionalHeader.Offset + FileHeader.SizeOfOptionalHeader;
            _sectionHeaders = new List<SectionHeader>(FileHeader.NumberOfSections);
            for (int i = 0; i < FileHeader.NumberOfSections; i++)
                _sectionHeaders.Add(SectionHeader.FromReader(reader));

            // Data between section headers and sections.
            int extraSectionDataLength = (int) (OptionalHeader.SizeOfHeaders - reader.Offset);
            if (extraSectionDataLength != 0)
                ExtraSectionData = DataSegment.FromReader(reader, extraSectionDataLength);
        }

        /// <inheritdoc />
        protected override IList<PESection> GetSections()
        {
            var result = new PESectionCollection(this);
            
            for (int i = 0; i < _sectionHeaders.Count; i++)
            {
                var header = _sectionHeaders[i];
                
                var contents = DataSegment.FromReader(_reader, (int) header.SizeOfRawData);
                contents.UpdateOffsets(header.PointerToRawData, header.VirtualAddress);
                result.Add(new PESection(header, new VirtualSegment(contents, header.VirtualSize)));
            }

            return result;
        }

    }
}