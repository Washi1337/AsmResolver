// AsmResolver - Executable file format inspection library 
// Copyright (C) 2016-2019 Washi
// 
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 3.0 of the License, or (at your option) any later version.
// 
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301  USA

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
    public class PEFile : IOffsetConverter
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

            // NT headers.
            var peFile = new PEFile
            {
                DosHeader = dosHeader,
                FileHeader = FileHeader.FromReader(reader),
                OptionalHeader = OptionalHeader.FromReader(reader)
            };

            // Section headers.
            reader.FileOffset = peFile.OptionalHeader.FileOffset + peFile.FileHeader.SizeOfOptionalHeader;
            for (int i = 0; i < peFile.FileHeader.NumberOfSections; i++)
            {
                var header = SectionHeader.FromReader(reader);
                
                var contentsReader = reader.Fork(header.PointerToRawData, header.SizeOfRawData);
                var contents = DataSegment.FromReader(contentsReader);
                contents.UpdateOffsets(header.PointerToRawData, header.VirtualAddress);
                
                peFile.Sections.Add(new PESection(header, contents));
            }
            
            // Data between section headers and sections.
            int extraSectionDataLength = (int) (peFile.OptionalHeader.SizeOfHeaders - reader.FileOffset);
            if (extraSectionDataLength != 0)
                peFile.ExtraSectionData = DataSegment.FromReader(reader, extraSectionDataLength);
            
            return peFile;
        }

        private PEFile()
        {
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
        } = new List<PESection>();


        /// <summary>
        /// Gets or sets the padding data in between the last section header and the first section.
        /// </summary>
        public IReadableSegment ExtraSectionData
        {
            get;
            set;
        }

        /// <inheritdoc />
        public uint FileOffsetToRva(uint fileOffset)
        {
            var section = Sections.FirstOrDefault(s => s.Header.ContainsFileOffset(fileOffset));
            if (section ==null)
                throw new ArgumentOutOfRangeException(nameof(fileOffset));
            return section.Header.FileOffsetToRva(fileOffset);
        }

        /// <inheritdoc />
        public uint RvaToFileOffset(uint rva)
        {
            var section = Sections.FirstOrDefault(s => s.Header.ContainsRva(rva));
            if (section ==null)
                throw new ArgumentOutOfRangeException(nameof(rva));
            return section.Header.RvaToFileOffset(rva);
        }

        public void UpdateHeaders()
        {
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

            var lastSection = Sections[Sections.Count - 1];
            OptionalHeader.SizeOfImage = lastSection.Header.VirtualAddress + lastSection.Header.VirtualSize;

        }

        public void AlignSections()
        {
            for (int i = 0; i < Sections.Count; i++)
            {
                var section = Sections[i];
                var header = section.Header;

                uint fileOffset = i > 0
                    ? Sections[i - 1].FileOffset + Sections[i - 1].GetPhysicalSize()
                    : OptionalHeader.SizeOfHeaders;
                uint rva = i > 0
                    ? Sections[i - 1].Rva + Sections[i - 1].GetVirtualSize()
                    : OptionalHeader.SizeOfHeaders.Align(OptionalHeader.SectionAlignment);

                header.PointerToRawData = fileOffset.Align(OptionalHeader.FileAlignment);
                header.SizeOfRawData = section.Contents.GetPhysicalSize().Align(OptionalHeader.FileAlignment);
                header.VirtualAddress = rva.Align(OptionalHeader.SectionAlignment);
                header.VirtualSize = section.Contents.GetVirtualSize().Align(OptionalHeader.SectionAlignment);

                section.UpdateOffsets(header.PointerToRawData, header.VirtualAddress);
            }
        }
        
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
                section.Header.Write(writer);

            // Data between section headers and sections.
            ExtraSectionData?.Write(writer);

            // Sections.
            writer.FileOffset = OptionalHeader.SizeOfHeaders;
            foreach (var section in Sections)
            {
                writer.FileOffset = section.Header.PointerToRawData;
                section.Contents.Write(writer);
                writer.Align(OptionalHeader.FileAlignment);
            }
        }
        
    }
}