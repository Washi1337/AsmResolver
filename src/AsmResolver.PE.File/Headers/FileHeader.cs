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

namespace AsmResolver.PE.File.Headers
{
    /// <summary>
    /// Represents the COFF file header of a portable executable file.
    /// </summary>
    public class FileHeader : ISegment
    {
        public static FileHeader FromReader(IBinaryStreamReader reader)
        {
            return new FileHeader
            {
                FileOffset = reader.FileOffset,
                Machine = (MachineType) reader.ReadUInt16(),
                NumberOfSections = reader.ReadUInt16(),
                TimeDateStamp = reader.ReadUInt32(),
                PointerToSymbolTable = reader.ReadUInt32(),
                NumberOfSymbols = reader.ReadUInt32(),
                SizeOfOptionalHeader = reader.ReadUInt16(),
                Characteristics = (Characteristics) reader.ReadUInt16()
            };
        }

        /// <summary>
        /// Gets or sets the machine the portable executable file is compiled for.
        /// </summary>
        public MachineType Machine
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the number of sections defined in the portable executable file.
        /// </summary>
        public ushort NumberOfSections
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the time stamp of the portable executable file.
        /// </summary>
        public uint TimeDateStamp
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the absolute file offset to the symbol table.
        /// </summary>
        public uint PointerToSymbolTable
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the number of symbols defined in the portable executable file.
        /// </summary>
        public uint NumberOfSymbols
        {
            get;
            set;
        }
        
        /// <summary>
        /// Gets or sets the size of the optional header.
        /// </summary>
        public ushort SizeOfOptionalHeader
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the characteristics of the portable executable file.
        /// </summary>
        public Characteristics Characteristics
        {
            get;
            set;
        }

        /// <inheritdoc />
        public uint FileOffset
        {
            get;
            private set;
        }

        /// <inheritdoc />
        uint ISegment.Rva => FileOffset;

        /// <inheritdoc />
        public void UpdateOffsets(uint newFileOffset, uint newRva)
        {
            FileOffset = newFileOffset;
        }

        /// <inheritdoc />
        public uint GetPhysicalSize()
        {
            return 2 * sizeof (ushort) +
                   3 * sizeof (uint) +
                   2 * sizeof (ushort);
        }

        /// <inheritdoc />
        public uint GetVirtualSize()
        {
            return GetPhysicalSize();
        }

        /// <inheritdoc />
        public void Write(IBinaryStreamWriter writer)
        {
            writer.WriteUInt16((ushort)Machine);
            writer.WriteUInt16(NumberOfSections);
            writer.WriteUInt32(TimeDateStamp);
            writer.WriteUInt32(PointerToSymbolTable);
            writer.WriteUInt32(NumberOfSymbols);
            writer.WriteUInt16(SizeOfOptionalHeader);
            writer.WriteUInt16((ushort)Characteristics);
        }
        
    }
}