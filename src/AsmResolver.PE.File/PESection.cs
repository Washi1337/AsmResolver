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

using AsmResolver.PE.File.Headers;

namespace AsmResolver.PE.File
{
    /// <summary>
    /// Represents a single section in a portable executable (PE) file.
    /// </summary>
    public class PESection : ISegment
    {
        /// <summary>
        /// Creates a new empty section.
        /// </summary>
        /// <param name="name">The name of the section.</param>
        /// <param name="characteristics">The section flags.</param>
        public PESection(string name, SectionFlags characteristics)
        {
            Header = new SectionHeader(name, characteristics);
        }
        
        /// <summary>
        /// Creates a new section with the provided contents.
        /// </summary>
        /// <param name="header">The header to associate to the section.</param>
        /// <param name="contents">The contents of the section.</param>
        public PESection(SectionHeader header, IReadableSegment contents)
        {
            Header = header;
            Contents = contents;
        }
        
        /// <summary>
        /// Gets or sets the header associated to the section.
        /// </summary>
        public SectionHeader Header
        {
            get;
        }

        /// <summary>
        /// Gets or sets the contents of the section.
        /// </summary>
        public IReadableSegment Contents
        {
            get;
            set;
        }

        /// <inheritdoc />
        public uint FileOffset => Contents?.FileOffset ?? Header.PointerToRawData;

        /// <inheritdoc />
        public uint Rva => Contents?.Rva ?? Header.VirtualAddress;

        /// <inheritdoc />
        public void UpdateOffsets(uint newFileOffset, uint newRva)
        {
            Contents.UpdateOffsets(newFileOffset, newRva);
        }

        /// <inheritdoc />
        public uint GetPhysicalSize()
        {
            return Contents.GetPhysicalSize();
        }

        /// <inheritdoc />
        public uint GetVirtualSize()
        {
            return Contents.GetVirtualSize();
        }

        /// <inheritdoc />
        public void Write(IBinaryStreamWriter writer)
        {
            Contents.Write(writer);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return Header.Name;
        }
        
    }
}