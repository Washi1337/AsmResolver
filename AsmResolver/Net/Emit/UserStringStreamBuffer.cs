using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AsmResolver.Net.Emit
{
    /// <summary>
    /// Represents a buffer for constructing a new user-defined strings metadata stream.
    /// </summary>
    public class UserStringStreamBuffer : MetadataStreamBuffer
    {
        private readonly Dictionary<string, uint> _stringOffsetMapping = new Dictionary<string, uint>();
        private uint _length;

        public UserStringStreamBuffer()
        {
            _length = 1;
        }

        public override string Name
        {
            get { return "#US"; }
        }

        public override uint Length
        {
            get { return FileSegment.Align(_length, 4); }
        }

        /// <summary>
        /// Gets or creates a new index for the given string.
        /// </summary>
        /// <param name="value">The string to get the index from.</param>
        /// <returns>The index.</returns>
        public uint GetStringOffset(string value)
        {
            uint offset;
            if (!_stringOffsetMapping.TryGetValue(value, out offset))
            {
                _stringOffsetMapping.Add(value, offset = _length);
                var byteCount = (uint)Encoding.Unicode.GetByteCount(value) + 1;
                _length += byteCount.GetCompressedSize() + byteCount;
            }
            return offset;
        }

        public override MetadataStream CreateStream()
        {
            using (var stream = new MemoryStream())
            {
                var writer = new BinaryStreamWriter(stream);
                writer.WriteByte(0);

                foreach (var value in _stringOffsetMapping.Keys)
                {
                    writer.WriteCompressedUInt32((uint)Encoding.Unicode.GetByteCount(value) + 1);
                    writer.WriteBytes(Encoding.Unicode.GetBytes(value));
                    writer.WriteByte(0);
                }
                
                return new UserStringStream(new MemoryStreamReader(stream.ToArray()));
            }
        }
    }
}