using System.Text;

namespace AsmResolver.PE.Win32Resources.Version
{
    /// <summary>
    /// Represents the raw resource table header.
    /// </summary>
    public class ResourceTableHeader : SegmentBase
    {
        /// <summary>
        /// Reads a single resource table header.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <returns>The table header.</returns>
        public static ResourceTableHeader FromReader(IBinaryStreamReader reader)
        {
            return new ResourceTableHeader
            {
                Length = reader.ReadUInt16(),
                ValueLength = reader.ReadUInt16(),
                Type = (ResourceValueType) reader.ReadUInt16(),
                Key = reader.ReadUnicodeString(),
            };
        }

        /// <summary>
        /// Computes the size of the raw header in bytes.
        /// </summary>
        /// <param name="key">The key of the entry.</param>
        /// <returns>The number of bytes.</returns>
        public static uint GetResourceHeaderSize(string key)
        {
            return (uint) (sizeof(ushort)
                           + sizeof(ushort)
                           + sizeof(ushort)
                           + Encoding.Unicode.GetByteCount(key) + sizeof(char));
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
        public ResourceValueType Type
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

        /// <inheritdoc />
        public override uint GetPhysicalSize() => GetResourceHeaderSize(Key);

        /// <inheritdoc />
        public override void Write(IBinaryStreamWriter writer)
        {
            writer.WriteUInt16(Length);
            writer.WriteUInt16(ValueLength);
            writer.WriteUInt16((ushort) Type);

            var buffer = new byte[Encoding.Unicode.GetByteCount(Key) + sizeof(char)];
            Encoding.Unicode.GetBytes(Key, 0, Key.Length, buffer, 0);

            writer.Align(4);
        }
    }
}