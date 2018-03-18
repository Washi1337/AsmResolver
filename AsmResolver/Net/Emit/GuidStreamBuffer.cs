using System;
using System.Collections.Generic;
using System.IO;

namespace AsmResolver.Net.Emit
{
    /// <summary>
    /// Represents a buffer for constructing a new GUID metadata stream.
    /// </summary>
    public class GuidStreamBuffer : MetadataStreamBuffer
    {
        private readonly IDictionary<Guid, uint> _guidOffsetMapping = new Dictionary<Guid, uint>();
        private readonly MetadataBuffer _parentBuffer;
        private uint _length;

        public GuidStreamBuffer(MetadataBuffer parentBuffer)
        {
            if (parentBuffer == null) 
                throw new ArgumentNullException("parentBuffer");
            _parentBuffer = parentBuffer;
        }

        public override string Name
        {
            get { return "#GUID"; }
        }

        public override uint Length
        {
            get { return _length; }
        }

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

        public override MetadataStream CreateStream()
        {
            using (var stream = new MemoryStream())
            {
                var writer = new BinaryStreamWriter(stream);
                foreach (var guid in _guidOffsetMapping.Keys)
                    writer.WriteBytes(guid.ToByteArray());
                return new GuidStream(new MemoryStreamReader(stream.ToArray()));
            }
        }
    }
}