 using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsmResolver.Net
{
    /// <summary>
    /// Represents a user-defined strings storage stream (#US) in a .NET assembly image.
    /// </summary>
    public class UserStringStream : MetadataStream<UserStringStreamBuffer>
    {
        internal static UserStringStream FromReadingContext(ReadingContext context)
        {
            return new UserStringStream(context.Reader);
        }

        private readonly Dictionary<uint, string> _cachedStrings = new Dictionary<uint, string>();
        private readonly IBinaryStreamReader _reader;
        private readonly uint _length;
        private bool _hasReadAllStrings;

        public UserStringStream()
        {
        }

        internal UserStringStream(IBinaryStreamReader reader)
        {
            _reader = reader;
            _length = (uint)reader.Length;
        }

        /// <summary>
        /// Gets the string at the given offset.
        /// </summary>
        /// <param name="offset">The offset of the string to get.</param>
        /// <returns>The string.</returns>
        public string GetStringByOffset(uint offset)
        {
            if (offset == 0)
                return string.Empty;
            var reader = _reader.CreateSubReader(_reader.StartPosition);
            reader.Position += offset;
            lock (_cachedStrings)
            {
                return ReadString(reader);
            }
        }

        protected string ReadString(IBinaryStreamReader reader)
        {
            var offset = (uint)(reader.Position - reader.StartPosition);

            string value;
            if (_cachedStrings.TryGetValue(offset, out value))
                reader.Position += 1 + Encoding.Unicode.GetByteCount(value) + 1;
            else
            {
                var length = reader.ReadCompressedUInt32();
                if (length <= 0)
                    return string.Empty;

                value = Encoding.Unicode.GetString(reader.ReadBytes((int) length - 1));
                reader.Position++;

                _cachedStrings.Add(offset, value);
            }
            
            return value;
        }

        /// <summary>
        /// Enumerates all strings stored in the stream.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> EnumerateStrings()
        {
            if (_hasReadAllStrings)
                return _cachedStrings.Values;
            else
                return GetStringsEnumerator();
        }

        protected IEnumerable<string> GetStringsEnumerator()
        {
            var reader = _reader.CreateSubReader(_reader.StartPosition, (int)_reader.Length);
            reader.Position++;

            lock (_cachedStrings)
            {
                while (reader.Position < reader.StartPosition + reader.Length)
                {
                    var value = ReadString(reader);
                    if (!string.IsNullOrEmpty(value))
                        yield return value;
                }
            }

            _hasReadAllStrings = true;
        }
        
        /// <summary>
        /// Creates a new buffer for constructing a new user-strings storage stream.
        /// </summary>
        /// <returns></returns>
        public override UserStringStreamBuffer CreateBuffer()
        {
            return new UserStringStreamBuffer();
        }

        public override uint GetPhysicalLength()
        {
            return Align(_length, 4);
        }

        public override void Write(WritingContext context)
        {
            var writer = context.Writer;
            writer.WriteByte(0);

            foreach (var value in EnumerateStrings())
            {
                writer.WriteCompressedUInt32((uint)Encoding.Unicode.GetByteCount(value) + 1);
                writer.WriteBytes(Encoding.Unicode.GetBytes(value));
                writer.WriteByte(0);
            }
        }
    }

    /// <summary>
    /// Represents a buffer for constructing a new user-defined strings metadata stream.
    /// </summary>
    public class UserStringStreamBuffer : FileSegment
    {
        private readonly Dictionary<string, uint> _stringOffsetMapping = new Dictionary<string, uint>();
        private uint _length;

        public UserStringStreamBuffer()
        {
            _length = 1;
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

        public override uint GetPhysicalLength()
        {
            return Align(_length, 4);
        }

        public override void Write(WritingContext context)
        {
            var writer = context.Writer;
            writer.WriteByte(0);

            foreach (var value in _stringOffsetMapping.Keys)
            {
                writer.WriteCompressedUInt32((uint)Encoding.Unicode.GetByteCount(value) + 1);
                writer.WriteBytes(Encoding.Unicode.GetBytes(value));
                writer.WriteByte(0);
            }
        }
    }
}
