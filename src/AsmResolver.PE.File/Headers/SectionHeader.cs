using System;
using System.Text;
using AsmResolver.IO;

namespace AsmResolver.PE.File.Headers
{
    /// <summary>
    /// Represents a single section header in the portable executable (PE) file format.
    /// </summary>
    public class SectionHeader : SegmentBase, IOffsetConverter
    {
        /// <summary>
        /// Indicates the static size of a single section header.
        /// </summary>
        public const uint SectionHeaderSize = 8 * sizeof(byte) +
                                             6 * sizeof (uint) +
                                             2 * sizeof (ushort) +
                                             1 * sizeof (uint);

        private Utf8String _name;

        /// <summary>
        /// Creates a new section header with the provided name.
        /// </summary>
        /// <param name="name">The name of the new section.</param>
        /// <param name="characteristics">The section flags to assign.</param>
        public SectionHeader(Utf8String name, SectionFlags characteristics)
        {
            AssertIsValidName(name);
            _name = name;
            Characteristics = characteristics;
        }

        /// <summary>
        /// Creates a copy of the provided section header.
        /// </summary>
        /// <param name="value">The section header to base information on.</param>
        public SectionHeader(SectionHeader value)
        {
            Offset = value.Offset;
            Rva = value.Rva;

            _name = value.Name;
            VirtualSize = value.VirtualSize;
            VirtualAddress = value.VirtualAddress;
            SizeOfRawData = value.SizeOfRawData;
            PointerToRawData = value.PointerToRawData;
            PointerToRelocations = value.PointerToRelocations;
            PointerToLineNumbers = value.PointerToLineNumbers;
            NumberOfRelocations = value.NumberOfRelocations;
            NumberOfLineNumbers = value.NumberOfLineNumbers;
            Characteristics = value.Characteristics;
        }

        /// <summary>
        /// Gets or sets the name of the section.
        /// </summary>
        /// <remarks>
        /// The name of the section is a UTF-8 string that can be no longer than 8 characters long.
        /// </remarks>
        public Utf8String Name
        {
            get => _name;
            set
            {
                AssertIsValidName(value);
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

        /// <summary>
        /// Reads a single section header at the current position of the provided input stream.
        /// </summary>
        /// <param name="reader">The input stream to read from.</param>
        /// <returns>The section header that was read.</returns>
        public static SectionHeader FromReader(ref BinaryStreamReader reader)
        {
            ulong offset = reader.Offset;
            uint rva = reader.Rva;

            // Read name field.
            byte[] nameBytes = new byte[8];
            reader.ReadBytes(nameBytes, 0, nameBytes.Length);
            var nameReader = new BinaryStreamReader(nameBytes);

            // Interpret as UTF-8, discarding all invalid UTF-8 chgitaracters.
            var name = nameReader.ReadUtf8String();

            return new SectionHeader(name, 0)
            {
                Offset = offset,
                Rva = rva,

                // Read remainder of section header.
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

        /// <inheritdoc />
        public override uint GetPhysicalSize() => SectionHeaderSize;

        /// <summary>
        /// Determines whether the provided file offset falls within the section that the header describes.
        /// </summary>
        /// <param name="fileOffset">The offset to check.</param>
        /// <returns><c>true</c> if the file offset falls within the section, <c>false</c> otherwise.</returns>
        public bool ContainsFileOffset(ulong fileOffset)
        {
            return PointerToRawData <= fileOffset && fileOffset < PointerToRawData + SizeOfRawData;
        }

        /// <summary>
        /// Determines whether the provided virtual address falls within the section that the header describes.
        /// </summary>
        /// <param name="rva">The virtual address to check.</param>
        /// <returns><c>true</c> if the virtual address falls within the section, <c>false</c> otherwise.</returns>
        public bool ContainsRva(uint rva)
        {
            return VirtualAddress <= rva && rva < VirtualAddress + VirtualSize;
        }

        /// <inheritdoc />
        public uint FileOffsetToRva(ulong fileOffset)
        {
            if (!ContainsFileOffset(fileOffset))
                throw new ArgumentOutOfRangeException(nameof(fileOffset));
            return (uint) (fileOffset - PointerToRawData + VirtualAddress);
        }

        /// <inheritdoc />
        public ulong RvaToFileOffset(uint rva)
        {
            if (!ContainsRva(rva))
                throw new ArgumentOutOfRangeException(nameof(rva));
            return rva - VirtualAddress + PointerToRawData;
        }

        /// <inheritdoc />
        public override void Write(IBinaryStreamWriter writer)
        {
            writer.WriteBytes(Name.GetBytesUnsafe());
            writer.WriteZeroes(8 - Name.ByteCount);

            writer.WriteUInt32(VirtualSize);
            writer.WriteUInt32(VirtualAddress);
            writer.WriteUInt32(SizeOfRawData);
            writer.WriteUInt32(PointerToRawData);
            writer.WriteUInt32(PointerToRelocations);
            writer.WriteUInt32(PointerToLineNumbers);
            writer.WriteUInt16(NumberOfRelocations);
            writer.WriteUInt16(NumberOfLineNumbers);
            writer.WriteUInt32((uint) Characteristics);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Name} ({nameof(VirtualAddress)}: {VirtualAddress:X8}, " +
                   $"{nameof(VirtualSize)}: {VirtualSize:X8}, " +
                   $"{nameof(PointerToRawData)}: {PointerToRawData:X8}, " +
                   $"{nameof(SizeOfRawData)}: {SizeOfRawData:X8}, " +
                   $"{nameof(Characteristics)}: {Characteristics})";
        }

        internal static void AssertIsValidName(Utf8String value)
        {
            if (value.ByteCount > 8)
                throw new ArgumentException("Section name cannot be longer than 8 characters.");
        }
    }
}
