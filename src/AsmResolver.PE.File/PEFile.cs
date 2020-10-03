using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using AsmResolver.PE.File.Headers;

namespace AsmResolver.PE.File
{
    /// <summary>
    /// Models a file using the portable executable (PE) file format. It provides access to various PE headers, as well
    /// as the raw contents of each section present in the file. 
    /// </summary>
    public class PEFile : IPEFile
    {
        /// <summary>
        /// Indicates a valid NT header signature.
        /// </summary>
        public const uint ValidPESignature = 0x4550; // "PE\0\0"

        /// <summary>
        /// Reads an unmapped PE file from the disk.
        /// </summary>
        /// <param name="path">The file path to the PE file.</param>
        /// <returns>The PE file that was read.</returns>
        /// <exception cref="BadImageFormatException">Occurs when the file does not follow the PE file format.</exception>
        public static PEFile FromFile(string path) => FromReader(new ByteArrayReader(System.IO.File.ReadAllBytes(path)));

        /// <summary>
        /// Reads an unmapped PE file from memory.
        /// </summary>
        /// <param name="raw">The raw bytes representing the contents of the PE file to read.</param>
        /// <returns>The PE file that was read.</returns>
        /// <exception cref="BadImageFormatException">Occurs when the file does not follow the PE file format.</exception>
        public static PEFile FromBytes(byte[] raw) => FromReader(new ByteArrayReader(raw));

        /// <summary>
        /// Reads a PE file from the provided input stream.
        /// </summary>
        /// <param name="reader">The input stream to read from.</param>
        /// <param name="mode">Indicates how the input PE file is mapped.</param>
        /// <returns>The PE file that was read.</returns>
        /// <exception cref="BadImageFormatException">Occurs when the file does not follow the PE file format.</exception>
        public static PEFile FromReader(IBinaryStreamReader reader, PEMappingMode mode = PEMappingMode.Unmapped)
        {
            return new SerializedPEFile(reader, mode);
        }

        private readonly LazyVariable<ISegment> _extraSectionData;
        private IList<PESection> _sections;

        /// <summary>
        /// Creates a new empty portable executable file.
        /// </summary>
        public PEFile()
            : this(new DosHeader(), new FileHeader(), new OptionalHeader())
        {
        }

        /// <summary>
        /// Creates a new portable executable file.
        /// </summary>
        /// <param name="dosHeader">The DOS header to add.</param>
        /// <param name="fileHeader">The COFF header to add.</param>
        /// <param name="optionalHeader">The optional header to add.</param>
        public PEFile(DosHeader dosHeader, FileHeader fileHeader, OptionalHeader optionalHeader)
        {
            DosHeader = dosHeader ?? throw new ArgumentNullException(nameof(dosHeader));
            FileHeader = fileHeader ?? throw new ArgumentNullException(nameof(fileHeader));
            OptionalHeader = optionalHeader ?? throw new ArgumentNullException(nameof(optionalHeader));
            _extraSectionData = new LazyVariable<ISegment>(GetExtraSectionData);
            MappingMode = PEMappingMode.Unmapped;
        }

        /// <inheritdoc />
        public DosHeader DosHeader
        {
            get;
            set;
        }

        /// <inheritdoc />
        public FileHeader FileHeader
        {
            get;
            set;
        }

        /// <inheritdoc />
        public OptionalHeader OptionalHeader
        {
            get;
            set;
        }

        /// <inheritdoc />
        public IList<PESection> Sections
        {
            get
            {
                if (_sections is null)
                    Interlocked.CompareExchange(ref _sections, GetSections(), null);
                return _sections;
            }
        }

        /// <inheritdoc />
        public PEMappingMode MappingMode
        {
            get;
            protected set;
        }

        /// <summary>
        /// Gets or sets the padding data in between the last section header and the first section.
        /// </summary>
        public ISegment ExtraSectionData
        {
            get => _extraSectionData.Value;
            set => _extraSectionData.Value = value;
        }

        /// <inheritdoc />
        public ISegmentReference GetReferenceToRva(uint rva) => new PESegmentReference(this, rva);

        /// <inheritdoc />
        public uint FileOffsetToRva(ulong fileOffset) => 
            GetSectionContainingOffset(fileOffset).FileOffsetToRva(fileOffset);

        /// <inheritdoc />
        public ulong RvaToFileOffset(uint rva) => GetSectionContainingRva(rva).RvaToFileOffset(rva);

        /// <summary>
        /// Finds the section containing the provided file offset.
        /// </summary>
        /// <param name="fileOffset">The file offset.</param>
        /// <returns>The section containing the file offset.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Occurs when the file offset does not fall within any of the sections.</exception>
        public PESection GetSectionContainingOffset(ulong fileOffset)
        {
            if (!TryGetSectionContainingOffset(fileOffset, out var section))
                throw new ArgumentOutOfRangeException(nameof(fileOffset));
            return section;
        }

        /// <summary>
        /// Attempts to find the section containing the provided file offset.
        /// </summary>
        /// <param name="fileOffset">The file offset.</param>
        /// <param name="section">The section that was found.</param>
        /// <returns><c>true</c> if the section was found, <c>false</c> otherwise.</returns>
        public bool TryGetSectionContainingOffset(ulong fileOffset, out PESection section)
        {
            section = Sections.FirstOrDefault(s => s.ContainsFileOffset(fileOffset));
            return section != null;
        }

        /// <inheritdoc />
        public PESection GetSectionContainingRva(uint rva)
        {
            if (!TryGetSectionContainingRva(rva, out var section))
                throw new ArgumentOutOfRangeException(nameof(rva));
            return section;
        }

        /// <inheritdoc />
        public bool TryGetSectionContainingRva(uint rva, out PESection section)
        {
            section = Sections.FirstOrDefault(s => s.ContainsRva(rva));
            return section != null;
        }

        /// <inheritdoc />
        public IBinaryStreamReader CreateDataDirectoryReader(DataDirectory dataDirectory)
        {
            var section = GetSectionContainingRva(dataDirectory.VirtualAddress);
            ulong fileOffset = section.RvaToFileOffset(dataDirectory.VirtualAddress);
            return section.CreateReader(fileOffset, dataDirectory.Size);
        }

        /// <inheritdoc />
        public bool TryCreateDataDirectoryReader(DataDirectory dataDirectory, out IBinaryStreamReader reader)
        {
            if (TryGetSectionContainingRva(dataDirectory.VirtualAddress, out var section))
            {
                ulong fileOffset = section.RvaToFileOffset(dataDirectory.VirtualAddress);
                reader = section.CreateReader(fileOffset, dataDirectory.Size);
                return true;
            }

            reader = null;
            return false;
        }

        /// <inheritdoc />
        public IBinaryStreamReader CreateReaderAtFileOffset(uint fileOffset)
        {
            var section = GetSectionContainingOffset(fileOffset);
            return section.CreateReader(fileOffset);
        }
        
        /// <inheritdoc />
        public bool TryCreateReaderAtFileOffset(uint fileOffset, out IBinaryStreamReader reader)
        {
            if (TryGetSectionContainingOffset(fileOffset, out var section))
            {
                reader = section.CreateReader(fileOffset);
                return true;
            }

            reader = null;
            return false;
        }

        /// <inheritdoc />
        public IBinaryStreamReader CreateReaderAtFileOffset(uint fileOffset, uint size)
        {
            var section = GetSectionContainingOffset(fileOffset);
            return section.CreateReader(fileOffset, size);
        } 
        
        /// <inheritdoc />
        public bool TryCreateReaderAtFileOffset(uint fileOffset, uint size, out IBinaryStreamReader reader)
        {
            if (TryGetSectionContainingOffset(fileOffset, out var section))
            {
                reader = section.CreateReader(fileOffset, size);
                return true;
            }

            reader = null;
            return false;
        }

        /// <inheritdoc />
        public IBinaryStreamReader CreateReaderAtRva(uint rva)
        {
            var section = GetSectionContainingRva(rva);
            return section.CreateReader(section.RvaToFileOffset(rva));
        }

        /// <inheritdoc />
        public bool TryCreateReaderAtRva(uint rva, out IBinaryStreamReader reader)
        {
            if (TryGetSectionContainingRva(rva, out var section))
            {
                reader = section.CreateReader(section.RvaToFileOffset(rva));
                return true;
            }

            reader = null;
            return false;
        }

        /// <inheritdoc />
        public IBinaryStreamReader CreateReaderAtRva(uint rva, uint size)
        {
            var section = GetSectionContainingRva(rva);
            return section.CreateReader(section.RvaToFileOffset(rva), size);
        }

        /// <inheritdoc />
        public bool TryCreateReaderAtRva(uint rva, uint size, out IBinaryStreamReader reader)
        {
            if (TryGetSectionContainingRva(rva, out var section))
            {
                reader = section.CreateReader(section.RvaToFileOffset(rva), size);
                return true;
            }

            reader = null;
            return false;
        }
        
        /// <summary>
        /// Recomputes file offsets and sizes in the file, optional and section headers.
        /// </summary>
        /// <remarks>
        /// Affected fields in the file header include:
        /// <list type="bullet">
        ///     <item>
        ///         <term>SizeOfOptionalHeader</term>
        ///     </item>
        /// </list>
        /// Affected fields in the optional header include:
        /// <list type="bullet">
        ///     <item>
        ///         <term>SizeOfHeaders</term>
        ///     </item>
        /// </list>
        /// Affected fields in the section header include:
        /// <list type="bullet">
        ///     <item>
        ///         <term>VirtualAddress</term>
        ///         <term>VirtualSize</term>
        ///         <term>PointerToRawData</term>
        ///         <term>SizeOfRawData</term>
        ///     </item>
        /// </list>
        /// </remarks>
        public void UpdateHeaders()
        {
            var oldSections = Sections.Select(_ => _.CreateHeader()).ToList();

            FileHeader.NumberOfSections = (ushort) Sections.Count;
            
            FileHeader.UpdateOffsets(
                DosHeader.NextHeaderOffset + 4, 
                DosHeader.NextHeaderOffset + 4);
            OptionalHeader.UpdateOffsets(
                FileHeader.Offset + FileHeader.GetPhysicalSize(),
                FileHeader.Rva + FileHeader.GetVirtualSize());

            FileHeader.SizeOfOptionalHeader = (ushort) OptionalHeader.GetPhysicalSize();
            OptionalHeader.SizeOfHeaders = (uint) (OptionalHeader.Offset
                                                   + FileHeader.SizeOfOptionalHeader
                                                   + SectionHeader.SectionHeaderSize * (uint) Sections.Count)
                .Align(OptionalHeader.FileAlignment);
            
            AlignSections();
            AlignDataDirectoryEntries(oldSections);

            var lastSection = Sections[Sections.Count - 1];
            OptionalHeader.SizeOfImage = lastSection.Rva
                                         + lastSection.GetVirtualSize().Align(OptionalHeader.SectionAlignment);
        }

        /// <summary>
        /// Aligns all sections according to the file and section alignment properties in the optional header. 
        /// </summary>
        public void AlignSections()
        {
            uint currentFileOffset = OptionalHeader.SizeOfHeaders;

            for (int i = 0; i < Sections.Count; i++)
            {
                var section = Sections[i];                

                uint rva = i > 0
                    ? Sections[i - 1].Rva + Sections[i - 1].GetVirtualSize()
                    : OptionalHeader.SizeOfHeaders.Align(OptionalHeader.SectionAlignment);

                currentFileOffset = currentFileOffset.Align(OptionalHeader.FileAlignment);
                section.UpdateOffsets(currentFileOffset, rva.Align(OptionalHeader.SectionAlignment));
                currentFileOffset += section.GetPhysicalSize();
            }
        }

        /// <summary>
        /// Aligns all data directories' virtual address according to the section header's ones. 
        /// </summary>
        public void AlignDataDirectoryEntries(IList<SectionHeader> oldHeaders) 
        {
            var dataDirectoryEntries = OptionalHeader.DataDirectories;
            for (int j = 0; j < dataDirectoryEntries.Count; j++)
            {
                var dataDirectory = dataDirectoryEntries[j];
                if (dataDirectory.IsPresentInPE)
                {
                    uint virtualAddress = dataDirectory.VirtualAddress;
                    for(int i = 0; i < oldHeaders.Count; i++)
                    {
                        var header = oldHeaders[i];
                        
                        // Locate section containing image directory. 
                        if (header.VirtualAddress <= virtualAddress && header.VirtualAddress + header.SizeOfRawData > virtualAddress)
                        {
                            // Calculate the delta between the new section.rva and the old one.
                            if (Sections[i].Rva >= header.VirtualAddress)
                            {
                                uint sectionRvaDelta = Sections[i].Rva - header.VirtualAddress;
                                virtualAddress += sectionRvaDelta;
                            }
                            else
                            {
                                uint sectionRvaDelta = header.VirtualAddress - Sections[i].Rva;
                                virtualAddress -= sectionRvaDelta;
                            }

                            dataDirectoryEntries[j] = new DataDirectory(virtualAddress, dataDirectory.Size);
                            break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Writes the PE file to a file on the disk.
        /// </summary>
        /// <param name="filePath">The path of the file.</param>
        public void Write(string filePath)
        {
            using var stream = System.IO.File.Create(filePath);
            Write(stream);
        }

        /// <summary>
        /// Writes the PE file to the provided output stream.
        /// </summary>
        /// <param name="stream">The output stream to write to.</param>
        public void Write(Stream stream) => Write(new BinaryStreamWriter(stream));
        
        /// <summary>
        /// Writes the PE file to the provided output stream.
        /// </summary>
        /// <param name="writer">The output stream to write to.</param>
        public void Write(IBinaryStreamWriter writer)
        {
            UpdateHeaders();
            
            // Dos header.
            DosHeader.Write(writer);
            
            // NT headers
            writer.Offset = DosHeader.NextHeaderOffset;
            
            writer.WriteUInt32(ValidPESignature);
            FileHeader.Write(writer);
            OptionalHeader.Write(writer);

            // Section headers.
            writer.Offset = OptionalHeader.Offset + FileHeader.SizeOfOptionalHeader;
            foreach (var section in Sections) 
                section.CreateHeader().Write(writer);

            // Data between section headers and sections.
            ExtraSectionData?.Write(writer);

            // Sections.
            
            writer.Offset = OptionalHeader.SizeOfHeaders;
            foreach (var section in Sections)
            {
                writer.Offset = section.Offset;
                section.Contents?.Write(writer);
                writer.Align(OptionalHeader.FileAlignment);
            }
        }

        /// <summary>
        /// Obtains the sections in the portable executable file.
        /// </summary>
        /// <returns>The section.</returns>
        /// <remarks>
        /// This method is called upon the initialization of the <see cref="Sections"/> property.
        /// </remarks>
        protected virtual IList<PESection> GetSections() => new PESectionCollection(this);

        /// <summary>
        /// Obtains the padding data in between the last section header and the first section.
        /// </summary>
        /// <returns>The extra padding data.</returns>
        /// <remarks>
        /// This method is called upon the initialization of the <see cref="ExtraSectionData"/> property.
        /// </remarks>
        protected virtual ISegment GetExtraSectionData() => null;
    }
}