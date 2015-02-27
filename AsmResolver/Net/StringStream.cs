using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace AsmResolver.Net
{
    public class StringStream : MetadataStream<StringStreamBuffer>
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

        public string GetStringByOffset(uint offset)
        {
            if (offset == 0)
                return string.Empty;

            var reader = _reader.CreateSubReader(_reader.StartPosition);
            reader.Position += offset;
            return ReadString(reader);
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

        public IEnumerable<string> EnumerateStrings()
        {
            return _hasReadAllStrings ? _cachedStrings.Values : GetStringsEnumerator();
        }

        protected IEnumerable<string> GetStringsEnumerator()
        {
            var reader = _reader.CreateSubReader(_reader.StartPosition, (int)_reader.Length);
            reader.Position++;
            while (reader.Position < reader.StartPosition + reader.Length)
            {
                yield return ReadString(reader);
            }

            _hasReadAllStrings = true;
        }

        // public uint GetStringOffset(string value)
        // {
        //     if (string.IsNullOrEmpty(value))
        //         return 0;
        // 
        //     if (!_hasReadAllStrings)
        //     {
        //         EnumerateStrings().ToArray();
        //         SortCachedStrings();
        //     }
        // 
        //     if (_cachedStrings.ContainsValue(value))
        //         return _cachedStrings.First(x => x.Value == value).Key;
        //     throw new ArgumentException("String value is not present in the string stream.");
        // }

        public override StringStreamBuffer CreateBuffer()
        {
            return new StringStreamBuffer();
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

    public class StringStreamBuffer : FileSegment
    {
        private readonly Dictionary<string, uint> _stringOffsetMapping = new Dictionary<string, uint>();
        private uint _length;

        public StringStreamBuffer()
        {
            _length = 1;
        }

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

        public override uint GetPhysicalLength()
        {
            return _length;
        }

        public override void Write(WritingContext context)
        {
            var writer = context.Writer;
            writer.WriteByte(0);

            foreach (var value in _stringOffsetMapping.Keys)
            {
                writer.WriteBytes(Encoding.UTF8.GetBytes(value));
                writer.WriteByte(0);
            }

            writer.WriteByte(0);
        }
    }
}
