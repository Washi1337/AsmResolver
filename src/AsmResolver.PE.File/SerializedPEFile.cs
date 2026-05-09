using System;
using System.Collections.Generic;
using AsmResolver.IO;

namespace AsmResolver.PE.File
{
    /// <summary>
    /// Provides an implementation of a PE image that gets its data from an input stream.
    /// </summary>
    public class SerializedPEFile : PEFile
    {
        private readonly List<SectionHeader> _sectionHeaders;
        private readonly BinaryStreamReaderState _readerState;
        private readonly ulong _originalImageBase;
        private readonly bool _is32Bit;

        /// <summary>
        /// Reads a PE file from an input stream.
        /// </summary>
        /// <param name="reader">The input stream.</param>
        /// <param name="mode">Indicates how the input PE file is mapped.</param>
        /// <exception cref="BadImageFormatException">Occurs when the input stream is malformed.</exception>
        public SerializedPEFile(in BinaryStreamReaderState readerState, PEMappingMode mode)
        {
            _readerState = readerState;
            MappingMode = mode;

            var reader = readerState.CreateReader();

            // DOS header.
            DosHeader = DosHeader.FromReader(ref reader);
            reader.Offset = DosHeader.Offset + DosHeader.NextHeaderOffset;

            uint signature = reader.ReadUInt32();
            if (signature != ValidPESignature)
                throw new BadImageFormatException();

            // Read NT headers.
            FileHeader = FileHeader.FromReader(ref reader);
            OptionalHeader = OptionalHeader.FromReader(ref reader);
            _originalImageBase = OptionalHeader.ImageBase;
            _is32Bit = OptionalHeader.Magic == OptionalHeaderMagic.PE32;

            // Read section headers.
            reader.Offset = OptionalHeader.Offset + FileHeader.SizeOfOptionalHeader;
            _sectionHeaders = new List<SectionHeader>(FileHeader.NumberOfSections);
            for (int i = 0; i < FileHeader.NumberOfSections; i++)
                _sectionHeaders.Add(SectionHeader.FromReader(ref reader));

            // Data between section headers and sections.
            int extraSectionDataLength = (int) (DosHeader.Offset + OptionalHeader.SizeOfHeaders - reader.Offset);
            if (extraSectionDataLength != 0)
                ExtraSectionData = reader.ReadSegment((uint) extraSectionDataLength);
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
                    PEMappingMode.Unmapped => (_readerState.StartOffset + header.PointerToRawData, header.SizeOfRawData),
                    PEMappingMode.Mapped => (_readerState.StartOffset + header.VirtualAddress, header.VirtualSize),
                    _ => throw new ArgumentOutOfRangeException()
                };

                ISegment? physicalContents = null;
                if (size > 0)
                    physicalContents = new DataSourceSegment(_readerState.DataSource, offset, header.VirtualAddress, size);

                var virtualSegment = new VirtualSegment(physicalContents, header.VirtualSize);
                virtualSegment.UpdateOffsets(new RelocationParameters(
                    _originalImageBase,
                    offset,
                    header.VirtualAddress,
                    _is32Bit));

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

            var reader = _readerState.WithOffset(offset).CreateReader();
            return reader.Length > 0
                ? reader.ReadSegment(reader.Length)
                : null;
        }
    }
}
