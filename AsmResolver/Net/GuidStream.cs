using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsmResolver.Net
{
    /// <summary>
    /// Represents a GUID storage stream (#GUID) in a .NET assembly image.
    /// </summary>
    public class GuidStream : MetadataStream<GuidStreamBuffer>
    {
        internal static GuidStream FromReadingContext(ReadingContext context)
        {
            return new GuidStream(context.Reader);
        }

        private readonly Dictionary<uint, Guid> _cachedGuids = new Dictionary<uint, Guid>();
        private readonly IBinaryStreamReader _reader;
        private bool _hasReadAllGuids;

        public GuidStream()
        {
        }

        internal GuidStream(IBinaryStreamReader reader)
        {
            _reader = reader;
        }

        /// <summary>
        /// Gets the GUID at the given index.
        /// </summary>
        /// <param name="offset">The index of the GUID to get.</param>
        /// <returns>The GUID.</returns>
        public Guid GetGuidByOffset(uint offset)
        {
            if (offset == 0)
                return Guid.Empty;
            lock (_cachedGuids)
            {
                return ReadGuid(_reader.CreateSubReader(_reader.StartPosition + offset - 1));
            }
        }

        private Guid ReadGuid(IBinaryStreamReader reader)
        {
            Guid guid;
            var offset = (uint) (reader.Position - reader.StartPosition) + 1;

            if (!_cachedGuids.TryGetValue(offset, out guid))
                _cachedGuids.Add(offset, guid = new Guid(reader.ReadBytes(16)));
            else
                reader.Position += 16;

            return guid;
        }

        /// <summary>
        /// Enumerates all GUIDs in the storage stream.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Guid> EnumerateGuids()
        {
            if (_hasReadAllGuids)
                return _cachedGuids.Values;
            else
                return GetGuidEnumerator();
        }

        private IEnumerable<Guid> GetGuidEnumerator()
        {
            var reader = _reader.CreateSubReader(_reader.StartPosition, (int)_reader.Length);

            while (reader.Position < reader.StartPosition + reader.Length)
            {
                yield return ReadGuid(reader);
            }

            _hasReadAllGuids = true;
        }

        /// <summary>
        /// Creates a new buffer for constructing a new GUID storage stream.
        /// </summary>
        /// <returns></returns>
        public override GuidStreamBuffer CreateBuffer()
        {
            return new GuidStreamBuffer();
        }

        public override uint GetPhysicalLength()
        {
            return (uint)_reader.Length;
        }

        public override void Write(WritingContext context)
        {
            foreach (var value in EnumerateGuids())
                context.Writer.WriteBytes(value.ToByteArray());
        }
    }

    /// <summary>
    /// Represents a buffer for constructing a new GUID metadata stream.
    /// </summary>
    public class GuidStreamBuffer : FileSegment
    {
        private readonly Dictionary<Guid, uint> _guidOffsetMapping = new Dictionary<Guid, uint>();
        private uint _length;

        /// <summary>
        /// Gets or creates a new index for the given GUID.
        /// </summary>
        /// <param name="guid">The GUID to get the index from.</param>
        /// <returns>The index.</returns>
        public uint GetGuidOffset(Guid guid)
        {
            if (guid == default(Guid))
                return 0;

            uint offset;
            if (!_guidOffsetMapping.TryGetValue(guid, out offset))
            {
                _guidOffsetMapping.Add(guid, offset = _length + 1);
                _length += 16;
            }
            return offset;
        }

        public override uint GetPhysicalLength()
        {
            return _length;
        }

        public override void Write(WritingContext context)
        {
            foreach (var guid in _guidOffsetMapping.Keys)
                context.Writer.WriteBytes(guid.ToByteArray());
        }
    }
}
