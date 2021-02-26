using System;
using System.Text;

namespace AsmResolver.PE.Win32Resources.Version
{
    /// <summary>
    /// Represents the raw version table entry header.
    /// </summary>
    public class VersionTableEntryHeader : SegmentBase
    {
        /// <summary>
        /// Reads a single resource table header.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <returns>The table header.</returns>
        public static VersionTableEntryHeader FromReader(IBinaryStreamReader reader)
        {
            return new VersionTableEntryHeader
            {
                Length = reader.ReadUInt16(),
                ValueLength = reader.ReadUInt16(),
                Type = (VersionTableValueType) reader.ReadUInt16(),
                Key = reader.ReadUnicodeString(),
            };
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
        public string? Key
        {
            get;
            set;
        }

        /// <inheritdoc />
        public override uint GetPhysicalSize()
        {
            ThrowIfKeyNull();
            return GetHeaderSize(Key!);
        }

        /// <inheritdoc />
        public override void Write(IBinaryStreamWriter writer)
        {
            ThrowIfKeyNull();

            writer.WriteUInt16(Length);
            writer.WriteUInt16(ValueLength);
            writer.WriteUInt16((ushort) Type);
            writer.WriteBytes(Encoding.Unicode.GetBytes(Key!));
            writer.WriteUInt16(0);
        }

        private void ThrowIfKeyNull()
        {
            if (Key == null)
                throw new InvalidOperationException($"{nameof(Key)} is not set.");
        }
    }
}
