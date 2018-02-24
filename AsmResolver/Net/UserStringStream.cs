using System.Collections.Generic;
using System.Text;

namespace AsmResolver.Net
{
    /// <summary>
    /// Represents a user-defined strings storage stream (#US) in a .NET assembly image.
    /// </summary>
    public class UserStringStream : MetadataStream
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

        public UserStringStream(IBinaryStreamReader reader)
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
}
