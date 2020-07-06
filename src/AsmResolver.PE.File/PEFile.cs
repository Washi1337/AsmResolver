using System;
using System.Collections.Generic;
using System.Linq;
using AsmResolver.PE.File.Headers;

namespace AsmResolver.PE.File
{
    /// <summary>
    /// Models a file using the portable executable (PE) file format. It provides access to various PE headers, as well
    /// as the raw contents of each section present in the file. 
    /// </summary>
    public class PEFile : ISegmentReferenceResolver, IOffsetConverter
    {
        public const uint ValidPESignature = 0x4550; // "PE\0\0"
        
        /// <summary>
        /// Reads a PE file from the disk.
        /// </summary>
        /// <param name="path">The file path to the PE file.</param>
        /// <returns>The PE file that was read.</returns>
        /// <exception cref="BadImageFormatException">Occurs when the file does not follow the PE file format.</exception>
        public static PEFile FromFile(string path)
        {
            return FromReader(new ByteArrayReader(System.IO.File.ReadAllBytes(path)));
        }

        /// <summary>
        /// Reads a PE file from memory.
        /// </summary>
        /// <param name="raw">The raw bytes representing the contents of the PE file to read.</param>
        /// <returns>The PE file that was read.</returns>
        /// <exception cref="BadImageFormatException">Occurs when the file does not follow the PE file format.</exception>
        public static PEFile FromBytes(byte[] raw)
        {
            return FromReader(new ByteArrayReader(raw));
        }
        
        /// <summary>
        /// Reads a PE file from the provided input stream.
        /// </summary>
        /// <param name="reader">The input stream to read from.</param>
        /// <returns>The PE file that was read.</returns>
        /// <exception cref="BadImageFormatException">Occurs when the file does not follow the PE file format.</exception>
        public static PEFile FromReader(IBinaryStreamReader reader)
        {
            // DOS header.
            var dosHeader = DosHeader.FromReader(reader);
            reader.FileOffset = dosHeader.NextHeaderOffset;

            uint signature = reader.ReadUInt32();
            if (signature != ValidPESignature)
                throw new BadImageFormatException();

            // Read NT headers.
            var peFile = new PEFile(
                dosHeader, 
                FileHeader.FromReader(reader), 
                OptionalHeader.FromReader(reader));

            // Section headers.
            reader.FileOffset = peFile.OptionalHeader.FileOffset + peFile.FileHeader.SizeOfOptionalHeader;
            for (int i = 0; i < peFile.FileHeader.NumberOfSections; i++)
            {
                var header = SectionHeader.FromReader(reader);
                
                var contentsReader = reader.Fork(header.PointerToRawData, header.SizeOfRawData);
                var contents = DataSegment.FromReader(contentsReader);
                contents.UpdateOffsets(header.PointerToRawData, header.VirtualAddress);

                peFile.Sections.Add(new PESection(header, new VirtualSegment(contents, header.VirtualSize)));
            }
            
            // Data between section headers and sections.
            int extraSectionDataLength = (int) (peFile.OptionalHeader.SizeOfHeaders - reader.FileOffset);
            if (extraSectionDataLength != 0)
                peFile.ExtraSectionData = DataSegment.FromReader(reader, extraSectionDataLength);
            
            return peFile;
        }

        public PEFile()
            : this(new DosHeader(), new FileHeader(), new OptionalHeader())
        {
        }

        public PEFile(DosHeader dosHeader, FileHeader fileHeader, OptionalHeader optionalHeader)
        {
            DosHeader = dosHeader ?? throw new ArgumentNullException(nameof(dosHeader));
            FileHeader = fileHeader ?? throw new ArgumentNullException(nameof(fileHeader));
            OptionalHeader = optionalHeader ?? throw new ArgumentNullException(nameof(optionalHeader));
            Sections = new PESectionCollection(this);
        }

        /// <summary>
        /// Gets or sets the DOS header of the PE file.
        /// </summary>
        public DosHeader DosHeader
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the COFF file header of the portable executable (PE) file.
        /// </summary>
        public FileHeader FileHeader
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the optional header of the portable executable (PE) file.
        /// </summary>
        public OptionalHeader OptionalHeader
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a collection of sections present in the portable executable (PE) file.
        /// </summary>
        public IList<PESection> Sections
        {
            get;
        }


        /// <summary>
        /// Gets or sets the padding data in between the last section header and the first section.
        /// </summary>
        public IReadableSegment ExtraSectionData
        {
            get;
            set;
        }

        /// <inheritdoc />
        public ISegmentReference GetReferenceToRva(uint rva) => new PESegmentReference(this, rva);

        /// <inheritdoc />
        public uint FileOffsetToRva(uint fileOffset) => 
            GetSectionContainingOffset(fileOffset).FileOffsetToRva(fileOffset);

        /// <inheritdoc />
        public uint RvaToFileOffset(uint rva) => GetSectionContainingRva(rva).RvaToFileOffset(rva);

        /// <summary>
        /// Finds the section containing the provided file offset.
        /// </summary>
        /// <param name="fileOffset">The file offset.</param>
        /// <returns>The section containing the file offset.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Occurs when the file offset does not fall within any of the sections.</exception>
        public PESection GetSectionContainingOffset(uint fileOffset)
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
        public bool TryGetSectionContainingOffset(uint fileOffset, out PESection section)
        {
            section = Sections.FirstOrDefault(s => s.ContainsFileOffset(fileOffset));
            return section != null;
        }

        /// <summary>
        /// Finds the section containing the provided virtual address.
        /// </summary>
        /// <param name="rva">The virtual address.</param>
        /// <returns>The section containing the virtual address.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Occurs when the virtual address does not fall within any of the sections.</exception>
        public PESection GetSectionContainingRva(uint rva)
        {
            if (!TryGetSectionContainingRva(rva, out var section))
                throw new ArgumentOutOfRangeException(nameof(rva));
            return section;
        }

        /// <summary>
        /// Attempts to find the section containing the provided virtual address.
        /// </summary>
        /// <param name="rva">The virtual address.</param>
        /// <param name="section">The section that was found.</param>
        /// <returns><c>true</c> if the section was found, <c>false</c> otherwise.</returns>
        public bool TryGetSectionContainingRva(uint rva, out PESection section)
        {
            section = Sections.FirstOrDefault(s => s.ContainsRva(rva));
            return section != null;
        }

        /// <summary>
        /// Obtains a reader that spans the provided data directory.
        /// </summary>
        /// <param name="dataDirectory">The data directory to read.</param>
        /// <returns>The reader.</returns>
        public IBinaryStreamReader CreateDataDirectoryReader(DataDirectory dataDirectory)
        {
            var section = GetSectionContainingRva(dataDirectory.VirtualAddress);
            uint fileOffset = section.RvaToFileOffset(dataDirectory.VirtualAddress);
            return section.CreateReader(fileOffset, dataDirectory.Size);
        }

        /// <summary>
        /// Attempts to create a reader that spans the provided data directory.
        /// </summary>
        /// <param name="dataDirectory">The data directory to read.</param>
        /// <param name="reader">The reader that was created.</param>
        /// <returns><c>true</c> if the reader was created successfully, <c>false</c> otherwise.</returns>
        public bool TryCreateDataDirectoryReader(DataDirectory dataDirectory, out IBinaryStreamReader reader)
        {
            if (TryGetSectionContainingRva(dataDirectory.VirtualAddress, out var section))
            {
                uint fileOffset = section.RvaToFileOffset(dataDirectory.VirtualAddress);
                reader = section.CreateReader(fileOffset, dataDirectory.Size);
                return true;
            }

            reader = null;
            return false;
        }

        /// <summary>
        /// Creates a new reader at the provided file offset.
        /// </summary>
        /// <param name="fileOffset">The file offset to start reading at.</param>
        /// <returns>The reader.</returns>
        public IBinaryStreamReader CreateReaderAtFileOffset(uint fileOffset)
        {
            var section = GetSectionContainingOffset(fileOffset);
            return section.CreateReader(fileOffset);
        }
        
        /// <summary>
        /// Attempts to create a new reader at the provided file offset.
        /// </summary>
        /// <param name="fileOffset">The file offset to start reading at.</param>
        /// <param name="reader">The reader that was created.</param>
        /// <returns><c>true</c> if the reader was created successfully, <c>false</c> otherwise.</returns>
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

        /// <summary>
        /// Creates a new reader of a chunk of data at the provided file offset.
        /// </summary>
        /// <param name="fileOffset">The file offset to start reading at.</param>
        /// <param name="size">The number of bytes in the chunk.</param>
        /// <returns>The reader.</returns>
        public IBinaryStreamReader CreateReaderAtFileOffset(uint fileOffset, uint size)
        {
            var section = GetSectionContainingOffset(fileOffset);
            return section.CreateReader(fileOffset, size);
        } 
        
        /// <summary>
        /// Attempts to create a new reader of a chunk of data at the provided file offset.
        /// </summary>
        /// <param name="fileOffset">The file offset to start reading at.</param>
        /// <param name="size">The number of bytes in the chunk.</param>
        /// <param name="reader">The reader that was created.</param>
        /// <returns><c>true</c> if the reader was created successfully, <c>false</c> otherwise.</returns>
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

        /// <summary>
        /// Creates a new reader at the provided virtual address.
        /// </summary>
        /// <param name="rva">The virtual address to start reading at.</param>
        /// <returns>The reader.</returns>
        public IBinaryStreamReader CreateReaderAtRva(uint rva)
        {
            var section = GetSectionContainingRva(rva);
            return section.CreateReader(section.RvaToFileOffset(rva));
        }

        /// <summary>
        /// Attempts to create a new reader at the provided virtual address.
        /// </summary>
        /// <param name="rva">The virtual address to start reading at.</param>
        /// <param name="reader">The reader that was created.</param>
        /// <returns><c>true</c> if the reader was created successfully, <c>false</c> otherwise.</returns>
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

        /// <summary>
        /// Creates a new reader of a chunk of data at the provided virtual address.
        /// </summary>
        /// <param name="rva">The virtual address to start reading at.</param>
        /// <param name="size">The number of bytes in the chunk.</param>
        /// <returns>The reader.</returns>
        public IBinaryStreamReader CreateReaderAtRva(uint rva, uint size)
        {
            var section = GetSectionContainingRva(rva);
            return section.CreateReader(section.RvaToFileOffset(rva), size);
        }

        /// <summary>
        /// Attempts to create a new reader of a chunk of data at the provided virtual address.
        /// </summary>
        /// <param name="rva">The virtual address to start reading at.</param>
        /// <param name="size">The number of bytes in the chunk.</param>
        /// <param name="reader">The reader that was created.</param>
        /// <returns><c>true</c> if the reader was created successfully, <c>false</c> otherwise.</returns>
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
                FileHeader.FileOffset + FileHeader.GetPhysicalSize(),
                FileHeader.FileOffset + FileHeader.GetVirtualSize());

            FileHeader.SizeOfOptionalHeader = (ushort) OptionalHeader.GetPhysicalSize();
            OptionalHeader.SizeOfHeaders = (OptionalHeader.FileOffset
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
            for (int i = 0; i < Sections.Count; i++)
            {
                var section = Sections[i];

                uint fileOffset = i > 0
                    ? Sections[i - 1].FileOffset + Sections[i - 1].GetPhysicalSize()
                    : OptionalHeader.SizeOfHeaders;
                uint rva = i > 0
                    ? Sections[i - 1].Rva + Sections[i - 1].GetVirtualSize()
                    : OptionalHeader.SizeOfHeaders.Align(OptionalHeader.SectionAlignment);

                section.UpdateOffsets(
                    fileOffset.Align(OptionalHeader.FileAlignment),
                    rva.Align(OptionalHeader.SectionAlignment));
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
                    uint oldRvaDir = dataDirectory.VirtualAddress;
                    for(int i = 0; i < oldHeaders.Count; i++)
                    {
                        var header = oldHeaders[i];
                        /* Locate section containing image directory. */
                        if (header.VirtualAddress <= oldRvaDir && header.VirtualAddress + header.SizeOfRawData > oldRvaDir)
                        {
                            /* Calculate the delta between the new section.rva and the old one */
                            if (Sections[i].Rva >= header.VirtualAddress)
                            {
                                uint sectionRvaDelta = Sections[i].Rva - header.VirtualAddress;
                                dataDirectory.VirtualAddress += sectionRvaDelta;
                            }
                            else
                            {
                                uint sectionRvaDelta = header.VirtualAddress - Sections[i].Rva;
                                dataDirectory.VirtualAddress -= sectionRvaDelta;
                            }
                            break;
                        }
                    }
                }
            }
        }
        
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
            writer.FileOffset = DosHeader.NextHeaderOffset;
            
            writer.WriteUInt32(ValidPESignature);
            FileHeader.Write(writer);
            OptionalHeader.Write(writer);

            // Section headers.
            writer.FileOffset = OptionalHeader.FileOffset + FileHeader.SizeOfOptionalHeader;
            foreach (var section in Sections) 
                section.CreateHeader().Write(writer);

            // Data between section headers and sections.
            ExtraSectionData?.Write(writer);

            // Sections.
            
            writer.FileOffset = OptionalHeader.SizeOfHeaders;
            foreach (var section in Sections)
            {
                writer.FileOffset = section.FileOffset;
                section.Contents.Write(writer);
                writer.Align(OptionalHeader.FileAlignment);
            }
        }
    }
}