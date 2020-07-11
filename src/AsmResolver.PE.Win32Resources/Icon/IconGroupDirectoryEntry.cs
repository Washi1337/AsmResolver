using System;

namespace AsmResolver.PE.Win32Resources.Icon
{
    /// <summary>
    /// Represents a single group icon resource entry.
    /// </summary>
    public class IconGroupDirectoryEntry : SegmentBase
    {
        /// <summary>
        /// The id of the icon entry.
        /// </summary>
        public ushort Id
        {
            get;
            set;
        }

        /// <summary>
        /// The width of the icon.
        /// </summary>
        public byte Width
        {
            get;
            set;
        }

        /// <summary>
        /// The height of the icon.
        /// </summary>
        public byte Height
        {
            get;
            set;
        }

        /// <summary>
        /// The amount of colors.
        /// </summary>
        public byte ColorCount
        {
            get;
            set;
        }

        /// <summary>
        /// Reserved field. Must be 0.
        /// </summary>
        public byte Reserved
        {
            get;
            set;
        }

        /// <summary>
        /// The amount of color planes.
        /// </summary>
        public ushort ColorPlanes
        {
            get;
            set;
        }

        /// <summary>
        /// The amount of bits per pixel.
        /// </summary>
        public ushort PixelBitCount
        {
            get;
            set;
        }

        /// <summary>
        /// The length of the icon.
        /// </summary>
        public uint BytesInRes
        {
            get;
            set;
        }

        /// <summary>
        /// Reads an icon group resource entry from an input stream.
        /// </summary>
        /// <param name="reader">The input stream.</param>
        /// <returns>The parsed group icon resource entry.</returns>
        public static IconGroupDirectoryEntry FromReader(IBinaryStreamReader reader)
        {
            var entry = new IconGroupDirectoryEntry
            {
                FileOffset = reader.FileOffset,
                Rva = reader.Rva,
                Width = reader.ReadByte(),
                Height = reader.ReadByte(),
                ColorCount = reader.ReadByte(),
                Reserved = reader.ReadByte(),
                ColorPlanes = reader.ReadUInt16(),
                PixelBitCount = reader.ReadUInt16(),
                BytesInRes = reader.ReadUInt32(),
                Id = reader.ReadUInt16()
            };

            return entry;
        }

        /// <inheritdoc />
        public override uint GetPhysicalSize()
        {
            return sizeof(byte) // Width
                   + sizeof(byte) // Height
                   + sizeof(byte) // ColorCount
                   + sizeof(byte) // Reserved
                   + sizeof(ushort) // ColorPlanes
                   + sizeof(ushort) // PixelBitCount
                   + sizeof(uint) // BytesInRes
                   + sizeof(ushort); // Id

        }

        /// <inheritdoc />
        public override void Write(IBinaryStreamWriter writer)
        {
            writer.WriteByte(Width);
            writer.WriteByte((Height));
            writer.WriteByte(ColorCount);
            writer.WriteByte(Reserved);
            writer.WriteUInt16(ColorPlanes);
            writer.WriteUInt16(PixelBitCount);
            writer.WriteUInt32(BytesInRes);
            writer.WriteUInt16(Id);
        }
    }
}