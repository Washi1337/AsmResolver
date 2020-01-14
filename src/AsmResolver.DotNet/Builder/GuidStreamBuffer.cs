using System;
using System.Collections.Generic;
using System.IO;
using AsmResolver.PE.DotNet.Metadata;
using AsmResolver.PE.DotNet.Metadata.Blob;
using AsmResolver.PE.DotNet.Metadata.Guid;

namespace AsmResolver.DotNet.Builder
{
    /// <summary>
    /// Provides a mutable buffer for building up a GUID stream in a .NET portable executable. 
    /// </summary>
    public class GuidStreamBuffer : IMetadataStreamBuffer
    {
        private readonly MemoryStream _rawStream = new MemoryStream();
        private readonly BinaryStreamWriter _writer;
        private readonly IDictionary<Guid, uint> _guids = new Dictionary<Guid, uint>();

        /// <summary>
        /// Creates a new GUID stream buffer with the default GUID stream name.
        /// </summary>
        public GuidStreamBuffer()
            : this(GuidStream.DefaultName)
        {
        }

        /// <summary>
        /// Creates a new GUID stream buffer.
        /// </summary>
        /// <param name="name">The name of the stream.</param>
        public GuidStreamBuffer(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            _writer = new BinaryStreamWriter(_rawStream);
        }

        /// <inheritdoc />
        public string Name
        {
            get;
        }

        private uint AppendRawData(byte[] data)
        {
            uint offset = (uint) _rawStream.Length;
            _writer.WriteBytes(data, 0, data.Length);
            return offset;
        }
        
        /// <summary>
        /// Gets the index to the provided GUID. If the GUID. is not present in the buffer, it will be appended to the
        /// end of the stream.
        /// </summary>
        /// <param name="guid">The GUID. to lookup or add.</param>
        /// <returns>The index of the GUID.</returns>
        public uint GetGuidIndex(Guid guid)
        {
            if (guid == Guid.Empty)
                return 0;
            
            if (!_guids.TryGetValue(guid, out uint index))
            {
                index = (uint) _rawStream.Length / 16 + 1;
                 AppendRawData(guid.ToByteArray());
                _guids.Add(guid, index);
            }

            return index;
        }

        /// <inheritdoc />
        public IMetadataStream CreateStream()
        {
            return new SerializedGuidStream(Name, _rawStream.ToArray());
        }
    }
}