using System;
using System.Collections.Generic;
using AsmResolver.PE.File.Headers;

namespace AsmResolver.PE.File
{
    /// <summary>
    /// Provides an implementation of a PE image that gets its data from an input stream.
    /// </summary>
    public class SerializedPEFile : PEFile
    {
        private readonly IList<SectionHeader> _sectionHeaders;
        private readonly IBinaryStreamReader _reader;

        /// <summary>
        /// Reads a PE file from an input stream.
        /// </summary>
        /// <param name="reader">The input stream.</param>
        /// <param name="mode">Indicates how the input PE file is mapped.</param>
        /// <exception cref="BadImageFormatException">Occurs when the input stream is malformed.</exception>
        public SerializedPEFile(IBinaryStreamReader reader, PEMappingMode mode)
        {
            _reader = reader ?? throw new ArgumentNullException(nameof(reader));
            MappingMode = mode;

            // DOS header.
            DosHeader = DosHeader.FromReader(reader);
            reader.Offset = DosHeader.Offset + DosHeader.NextHeaderOffset;

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
            int extraSectionDataLength = (int) (DosHeader.Offset + OptionalHeader.SizeOfHeaders - reader.Offset);
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

                (ulong offset, uint size) = MappingMode switch
                {
                    PEMappingMode.Unmapped => (header.PointerToRawData, header.SizeOfRawData),
                    PEMappingMode.Mapped => (_reader.StartOffset + header.VirtualAddress, header.VirtualSize),
                    _ => throw new ArgumentOutOfRangeException()
                };
                
                _reader.Offset = offset;

                ISegment physicalContents = null;
                if (size > 0) 
                    physicalContents = DataSegment.FromReader(_reader, (int) size);
                
                var virtualSegment = new VirtualSegment(physicalContents, header.VirtualSize);
                virtualSegment.UpdateOffsets(offset, header.VirtualAddress);
                result.Add(new PESection(header, virtualSegment));
            }

            return result;
        }

    }
}