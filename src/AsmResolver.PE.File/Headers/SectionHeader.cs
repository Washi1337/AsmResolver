using System;
using System.Text;

namespace AsmResolver.PE.File.Headers
{
    /// <summary>
    /// Represents a single section header in the portable executable (PE) file format.
    /// </summary>
    public class SectionHeader : ISegment
    {
        private uint _fileOffset;
        private string _name;

        /// <summary>
        /// Reads a single section header at the current position of the provided input stream.
        /// </summary>
        /// <param name="reader">The input stream to read from.</param>
        /// <returns>The section header that was read.</returns>
        public static SectionHeader FromReader(IBinaryStreamReader reader)
        {
            uint offset = reader.FileOffset;
            
            var nameBytes = new byte[8];
            reader.ReadBytes(nameBytes, 0, nameBytes.Length);
            
            return new SectionHeader(Encoding.UTF8.GetString(nameBytes).TrimEnd())
            {
                _fileOffset = offset,
                VirtualSize = reader.ReadUInt32(),
                VirtualAddress = reader.ReadUInt32(),
                SizeOfRawData = reader.ReadUInt32(),
                PointerToRawData = reader.ReadUInt32(),
                PointerToRelocations = reader.ReadUInt32(),
                PointerToLineNumbers = reader.ReadUInt32(),
                NumberOfRelocations = reader.ReadUInt16(),
                NumberOfLineNumbers = reader.ReadUInt16(),
                Characteristics = (SectionFlags) reader.ReadUInt32()
            };
        }
        
        /// <summary>
        /// Creates a new section header with the provided name.
        /// </summary>
        /// <param name="name">The name of the new section.</param>
        public SectionHeader(string name)
        {
            Name = name;
        }

        /// <inheritdoc />
        uint ISegment.FileOffset => _fileOffset;

        /// <inheritdoc />
        uint ISegment.Rva => _fileOffset;

        /// <summary>
        /// Gets or sets the name of the section.
        /// </summary>
        /// <remarks>
        /// The name of the section is a UTF-8 string that can be no longer than 8 characters long.
        /// </remarks>
        public string Name
        {
            get => _name;
            set
            {
                if (Encoding.UTF8.GetByteCount(value) > 8)
                    throw new ArgumentException("Name is too long.");
                _name = value;
            }
        }

        /// <summary>
        /// Gets or sets the total number of bytes the section consists of when it is loaded into memory.
        /// </summary>
        /// <remarks>
        /// If the value is greater than <see cref="SizeOfRawData"/>, it is zero-padded when loaded into memory.
        /// </remarks>
        public uint VirtualSize
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the virtual address (relative to the image base) of the section.
        /// </summary>
        public uint VirtualAddress
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the number of bytes the section consists of on disk.
        /// </summary>
        public uint SizeOfRawData
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the file offset to the beginning of the section on disk.
        /// </summary>
        public uint PointerToRawData
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the file offset to the beginning of the relocations table for the section. 
        /// </summary>
        /// <remarks>
        /// This field is set to zero in a normal PE image. 
        /// </remarks>
        public uint PointerToRelocations
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the file offset to the beginning of the line numbers table for the section. 
        /// </summary>
        /// <remarks>
        /// This field is set to zero in a normal PE image.
        /// </remarks>
        public uint PointerToLineNumbers
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the number of relocations in the relocation table of the section.
        /// </summary>
        /// <remarks>
        /// This field is set to zero for executable files. 
        /// </remarks>
        public ushort NumberOfRelocations
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the total amount of line numbers table referenced by <see cref="PointerToLineNumbers" /> for the section. 
        /// </summary>
        /// <remarks>
        /// This field is not used in a normal PE image.
        /// </remarks>
        public ushort NumberOfLineNumbers
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the characteristics of the section.
        /// </summary>
        public SectionFlags Characteristics
        {
            get;
            set;
        }

        /// <inheritdoc />
        public void UpdateOffsets(uint newFileOffset, uint newRva)
        {
            _fileOffset = newFileOffset;
        }

        /// <inheritdoc />
        public uint GetPhysicalSize()
        {
            return 8 * sizeof(byte) +
                   6 * sizeof (uint) +
                   2 * sizeof (ushort) +
                   1 * sizeof (uint);
        }

        /// <inheritdoc />
        public uint GetVirtualSize()
        {
            return GetPhysicalSize();
        }

        /// <inheritdoc />
        public void Write(IBinaryStreamWriter writer)
        {
            var nameBytes = Encoding.UTF8.GetBytes(Name ?? string.Empty);
            writer.WriteBytes(nameBytes);
            writer.WriteZeroes(8 - nameBytes.Length);

            writer.WriteUInt32(VirtualSize);
            writer.WriteUInt32(VirtualAddress);
            writer.WriteUInt32(SizeOfRawData);
            writer.WriteUInt32(PointerToRawData);
            writer.WriteUInt32(PointerToRelocations);
            writer.WriteUInt32(PointerToLineNumbers);
            writer.WriteUInt16(NumberOfRelocations);
            writer.WriteUInt16(NumberOfLineNumbers);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return$"{Name} ({nameof(VirtualAddress)}: {VirtualAddress:X8}, {nameof(VirtualSize)}: {VirtualSize:X8}, {nameof(PointerToRawData)}: {PointerToRawData:X8}, {nameof(SizeOfRawData)}: {SizeOfRawData:X8})";
        }
        
    }
}