using System;
using AsmResolver.PE.File.Headers;

namespace AsmResolver.PE.File
{
    public class SerializedPEFile : PEFile
    {
        public SerializedPEFile(IBinaryStreamReader reader)
        {
            // DOS header.
            DosHeader = DosHeader.FromReader(reader);
            reader.FileOffset = DosHeader.NextHeaderOffset;

            uint signature = reader.ReadUInt32();
            if (signature != ValidPESignature)
                throw new BadImageFormatException();

            // Read NT headers. 
            FileHeader = FileHeader.FromReader(reader);
            OptionalHeader = OptionalHeader.FromReader(reader);

            // Section headers.
            reader.FileOffset = OptionalHeader.FileOffset + FileHeader.SizeOfOptionalHeader;
            for (int i = 0; i < FileHeader.NumberOfSections; i++)
            {
                var header = SectionHeader.FromReader(reader);
                
                var contentsReader = reader.Fork(header.PointerToRawData, header.SizeOfRawData);
                var contents = DataSegment.FromReader(contentsReader);
                contents.UpdateOffsets(header.PointerToRawData, header.VirtualAddress);

                Sections.Add(new PESection(header, new VirtualSegment(contents, header.VirtualSize)));
            }
            
            // Data between section headers and sections.
            int extraSectionDataLength = (int) (OptionalHeader.SizeOfHeaders - reader.FileOffset);
            if (extraSectionDataLength != 0)
                ExtraSectionData = DataSegment.FromReader(reader, extraSectionDataLength);
        }
    }
}