using System;
using System.Collections.Generic;
using AsmResolver.IO;
using AsmResolver.PE.File.Headers;

namespace AsmResolver.PE.File
{
    /// <summary>
    /// Provides an implementation of a PE image that gets its data from an input stream.
    /// </summary>
    public class SerializedPEFile : PEFile
    {
        private readonly List<SectionHeader> _sectionHeaders;
        private readonly BinaryStreamReader _reader;

        /// <summary>
        /// Reads a PE file from an input stream.
        /// </summary>
        /// <param name="reader">The input stream.</param>
        /// <param name="mode">Indicates how the input PE file is mapped.</param>
        /// <exception cref="BadImageFormatException">Occurs when the input stream is malformed.</exception>
        public SerializedPEFile(in BinaryStreamReader reader, PEMappingMode mode)
        {
            _reader = reader;

            MappingMode = mode;

            // DOS header.
            DosHeader = DosHeader.FromReader(ref _reader);
            _reader.Offset = DosHeader.Offset + DosHeader.NextHeaderOffset;

            uint signature = _reader.ReadUInt32();
            if (signature != ValidPESignature)
                throw new BadImageFormatException();

            // Read NT headers.
            FileHeader = FileHeader.FromReader(ref _reader);
            OptionalHeader = OptionalHeader.FromReader(ref _reader);

            // Read section headers.
            _reader.Offset = OptionalHeader.Offset + FileHeader.SizeOfOptionalHeader;
            _sectionHeaders = new List<SectionHeader>(FileHeader.NumberOfSections);
            for (int i = 0; i < FileHeader.NumberOfSections; i++)
                _sectionHeaders.Add(SectionHeader.FromReader(ref _reader));

            // Data between section headers and sections.
            int extraSectionDataLength = (int) (DosHeader.Offset + OptionalHeader.SizeOfHeaders - _reader.Offset);
            if (extraSectionDataLength != 0)
                ExtraSectionData = _reader.ReadSegment((uint) extraSectionDataLength);
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
                    PEMappingMode.Unmapped => (_reader.StartOffset + header.PointerToRawData, header.SizeOfRawData),
                    PEMappingMode.Mapped => (_reader.StartOffset + header.VirtualAddress, header.VirtualSize),
                    _ => throw new ArgumentOutOfRangeException()
                };

                ISegment? physicalContents = null;
                if (size > 0)
                    physicalContents = new DataSourceSegment(_reader.DataSource, offset, header.VirtualAddress, size);

                var virtualSegment = new VirtualSegment(physicalContents, header.VirtualSize);
                virtualSegment.UpdateOffsets(offset, header.VirtualAddress);
                result.Add(new PESection(header, virtualSegment));
            }

            return result;
        }

        /// <inheritdoc />
        protected override ISegment? GetEofData()
        {
            if (MappingMode != PEMappingMode.Unmapped)
                return null;

            var lastSection = _sectionHeaders[_sectionHeaders.Count - 1];
            ulong offset = lastSection.PointerToRawData + lastSection.SizeOfRawData;

            var reader = _reader.ForkAbsolute(offset);
            return reader.Length > 0
                ? reader.ReadSegment(reader.Length)
                : null;
        }
    }
}
