using System.Text;
using AsmResolver.IO;

namespace AsmResolver.PE.Win32Resources.Version
{
    /// <summary>
    /// Represents the raw version table entry header.
    /// </summary>
    public class VersionTableEntryHeader : SegmentBase
    {
        /// <summary>
        /// Creates a new version table entry header with the provided key.
        /// </summary>
        /// <param name="key">The name of the resource.</param>
        public VersionTableEntryHeader(string key)
        {
            Key = key;
        }

        /// <summary>
        /// Creates a new version table entry header with the provided key.
        /// </summary>
        /// <param name="length">The length in bytes of the structure.</param>
        /// <param name="valueLength">The size in bytes of the value member.</param>
        /// <param name="type">The type of data that is stored.</param>
        /// <param name="key">The name of the resource.</param>
        public VersionTableEntryHeader(ushort length, ushort valueLength, VersionTableValueType type, string key)
        {
            Length = length;
            ValueLength = valueLength;
            Type = type;
            Key = key;
        }

        /// <summary>
        /// Gets or sets the raw length in bytes of the structure.
        /// </summary>
        public ushort Length
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the raw length in bytes of the value member.
        /// </summary>
        public ushort ValueLength
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating the value is binary or textual.
        /// </summary>
        public VersionTableValueType Type
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the key name of the resource.
        /// </summary>
        public string Key
        {
            get;
            set;
        }

        /// <summary>
        /// Reads a single resource table header.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <returns>The table header.</returns>
        public static VersionTableEntryHeader FromReader(ref BinaryStreamReader reader)
        {
            return new VersionTableEntryHeader(
                reader.ReadUInt16(),
                reader.ReadUInt16(),
                (VersionTableValueType) reader.ReadUInt16(),
                reader.ReadUnicodeString());
        }

        /// <summary>
        /// Computes the size of the raw header in bytes.
        /// </summary>
        /// <param name="key">The key of the entry.</param>
        /// <returns>The number of bytes.</returns>
        public static uint GetHeaderSize(string key)
        {
            return (uint) (
                sizeof(ushort) // Length
                + sizeof(ushort) // ValueLength
                + sizeof(ushort) // Type
                + Encoding.Unicode.GetByteCount(key) + sizeof(char) // Key
            );
        }

        /// <inheritdoc />
        public override uint GetPhysicalSize() => GetHeaderSize(Key);

        /// <inheritdoc />
        public override void Write(IBinaryStreamWriter writer)
        {
            writer.WriteUInt16(Length);
            writer.WriteUInt16(ValueLength);
            writer.WriteUInt16((ushort) Type);
            writer.WriteBytes(Encoding.Unicode.GetBytes(Key));
            writer.WriteUInt16(0);
        }
    }
}
