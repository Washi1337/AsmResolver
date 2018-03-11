using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AsmResolver.Net
{
    /// <summary>
    /// Represents a string storage stream (#Strings) in a .NET assembly image.
    /// </summary>
    public class StringStream : MetadataStream
    {
        internal static StringStream FromReadingContext(ReadingContext context)
        {
            return new StringStream(context.Reader);
        }

        private readonly Dictionary<uint, string> _cachedStrings = new Dictionary<uint, string>();
        private readonly IBinaryStreamReader _reader;
        private bool _hasReadAllStrings;

        public StringStream()
        {
        }

        internal StringStream(IBinaryStreamReader reader)
        {
            _reader = reader;
        }
        
        /// <summary>
        /// Gets the string at the given offset.
        /// </summary>
        /// <param name="offset">The offset of the string to get.</param>
        /// <returns>The string.</returns>
        public string GetStringByOffset(uint offset)
        {
            if (offset == 0 || offset >= _reader.Length)
                return null;

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
                reader.Position += Encoding.UTF8.GetByteCount(value) + 1;
            else
            {
                value = Encoding.UTF8.GetString(reader.ReadBytesUntil(0));
                reader.Position++;
                _cachedStrings.Add(offset, value);
            }

            return value;
        }

        /// <summary>
        /// Enumerates all strings stored in the stream.
        /// </summary>
        /// <returns>The strings stored in the stream.</returns>
        public IEnumerable<string> EnumerateStrings()
        {
            return _hasReadAllStrings ? _cachedStrings.Values : GetStringsEnumerator();
        }

        protected IEnumerable<string> GetStringsEnumerator()
        {
            var reader = _reader.CreateSubReader(_reader.StartPosition, (int)_reader.Length);
            reader.Position++;
            lock (_cachedStrings)
            {
                while (reader.Position < reader.StartPosition + reader.Length)
                {
                    yield return ReadString(reader);
                }
            }

            _hasReadAllStrings = true;
        }

        public override uint GetPhysicalLength()
        {
            return (uint)_reader.Length;
        }

        public override void Write(WritingContext context)
        {
            var writer = context.Writer;
            writer.WriteByte(0);

            foreach (var value in EnumerateStrings())
            {
                writer.WriteBytes(Encoding.UTF8.GetBytes(value));
                writer.WriteByte(0);
            }

            writer.WriteByte(0);
        }

        private void SortCachedStrings()
        {
            var items = (from pair in _cachedStrings
                         orderby pair.Key ascending
                         select pair).ToArray();
            _cachedStrings.Clear();
            foreach (var item in items)
                _cachedStrings.Add(item.Key, item.Value);
        }
    }
}
