using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AsmResolver.Net.Emit
{
    /// <summary>
    /// Represents a buffer for constructing a new strings metadata stream.
    /// </summary>
    public class StringStreamBuffer : MetadataStreamBuffer
    {
        private readonly IDictionary<string, uint> _stringOffsetMapping = new Dictionary<string, uint>();
        private uint _length;
        
        public StringStreamBuffer()
        {
            _length = 1;
        }

        public override string Name
        {
            get { return "#Strings"; }
        }

        public override uint Length
        {
            get { return FileSegment.Align(_length, 4); }
        }

        /// <summary>
        /// Gets or creates a new index for the given string.
        /// </summary>
        /// <param name="value">The string to get the offset from.</param>
        /// <returns>The index.</returns>
        public uint GetStringOffset(string value)
        {
            if (string.IsNullOrEmpty(value))
                return 0;

            uint offset;
            if (!_stringOffsetMapping.TryGetValue(value, out offset))
            {
                _stringOffsetMapping.Add(value, offset = _length);
                _length += (uint)Encoding.UTF8.GetByteCount(value) + 1;
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
                    writer.WriteBytes(Encoding.UTF8.GetBytes(value));
                    writer.WriteByte(0);
                }

                writer.WriteZeroes((int)(FileSegment.Align(_length, 4) - _length));
                
                return new StringStream(new MemoryStreamReader(stream.ToArray()));
            }
        }
    }
}