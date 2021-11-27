using System;
using AsmResolver.IO;

namespace AsmResolver.PE.File.Headers
{
    /// <summary>
    /// Represents the DOS header (also known as the MZ header) in the portable executable (PE) file format.
    /// </summary>
    public class DosHeader : SegmentBase
    {
        /// <summary>
        /// Indicates the magic constant that every DOS header should start with.
        /// </summary>
        public const ushort ValidPEMagic = 0x5A4D;

        /// <summary>
        /// Indicates the minimal length of a valid DOS header in a portable executable file.
        /// </summary>
        public const int MinimalDosHeaderLength = 0x40;

        /// <summary>
        /// Indicates the offset of the e_flanew field in the DOS header.
        /// </summary>
        public const int NextHeaderFieldOffset = 0x3C;

        /// <summary>
        /// Indicates the default value of the e_flanew field in the DOS header.
        /// </summary>
        public const int DefaultNewHeaderOffset = 0x80;

        private static readonly byte[] DefaultDosHeader = {
            0x4D, 0x5A, 0x90, 0x00, 0x03, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00,
            0xFF, 0xFF, 0x00, 0x00, 0xB8, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x40, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x80, 0x00, 0x00, 0x00,
            0x0E, 0x1F, 0xBA, 0x0E, 0x00, 0xB4, 0x09, 0xCD,0x21, 0xB8, 0x01, 0x4C,
            0xCD, 0x21, 0x54, 0x68, 0x69, 0x73, 0x20, 0x70, 0x72, 0x6F, 0x67, 0x72,
            0x61, 0x6D, 0x20, 0x63, 0x61, 0x6E, 0x6E, 0x6F, 0x74, 0x20, 0x62, 0x65,
            0x20, 0x72, 0x75, 0x6E, 0x20, 0x69, 0x6E, 0x20, 0x44, 0x4F, 0x53, 0x20,
            0x6D, 0x6F, 0x64, 0x65, 0x2E, 0x0D, 0x0D, 0x0A, 0x24, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00
        };

        /// <summary>
        /// Reads a DOS header structure at the current position of the provided input stream.
        /// </summary>
        /// <param name="reader">The input stream to read from.</param>
        /// <returns>The read DOS header.</returns>
        /// <exception cref="BadImageFormatException">Occurs when the input stream does not point to a valid DOS header.</exception>
        public static DosHeader FromReader(ref BinaryStreamReader reader)
        {
            ulong offset = reader.Offset;
            uint rva = reader.Rva;

            var stub = new byte[DefaultNewHeaderOffset];

            ushort magic = reader.ReadUInt16();
            if (magic != ValidPEMagic)
                throw new BadImageFormatException();

            reader.Offset += NextHeaderFieldOffset - 2;
            uint nextHeaderOffset = reader.ReadUInt32();

            if (nextHeaderOffset != DefaultNewHeaderOffset)
                Array.Resize(ref stub, (int) nextHeaderOffset);

            reader.Offset -= NextHeaderFieldOffset + 4;
            reader.ReadBytes(stub, 0, stub.Length);

            return new DosHeader(stub)
            {
                Offset = offset,
                Rva = rva,
                NextHeaderOffset = nextHeaderOffset
            };
        }

        private readonly byte[] _stub;

        /// <summary>
        /// Creates a new DOS header with the default contents.
        /// </summary>
        public DosHeader()
            : this(DefaultDosHeader)
        {
            NextHeaderOffset = 0x80;
        }

        /// <summary>
        /// Creates a new DOS header with the provided contents.
        /// </summary>
        /// <param name="stub">The raw contents of the header.</param>
        /// <exception cref="BadImageFormatException">Occurs when the input data does not contain a valid DOS header.</exception>
        private DosHeader(byte[] stub)
        {
            _stub = stub ?? throw new ArgumentNullException(nameof(stub));
        }

        /// <summary>
        /// Gets or sets the offset to the next header (NT header).
        /// </summary>
        public uint NextHeaderOffset
        {
            get;
            set;
        }

        /// <inheritdoc />
        public override uint GetPhysicalSize() => (uint) _stub.Length;

        /// <inheritdoc />
        public override void Write(IBinaryStreamWriter writer)
        {
            writer.WriteBytes(_stub, 0, NextHeaderFieldOffset);
            writer.WriteUInt32(NextHeaderOffset);
            writer.WriteBytes(_stub, NextHeaderFieldOffset + 4, _stub.Length - NextHeaderFieldOffset - 4);
        }

    }
}
