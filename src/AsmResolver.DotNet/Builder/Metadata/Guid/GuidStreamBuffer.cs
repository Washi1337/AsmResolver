using System;
using System.Collections.Generic;
using System.IO;
using AsmResolver.PE.DotNet.Metadata;
using AsmResolver.PE.DotNet.Metadata.Guid;

namespace AsmResolver.DotNet.Builder.Metadata.Guid
{
    /// <summary>
    /// Provides a mutable buffer for building up a GUID stream in a .NET portable executable. 
    /// </summary>
    public class GuidStreamBuffer : IMetadataStreamBuffer
    {
        private readonly MemoryStream _rawStream = new MemoryStream();
        private readonly BinaryStreamWriter _writer;
        private readonly IDictionary<System.Guid, uint> _guids = new Dictionary<System.Guid, uint>();

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
        public uint GetGuidIndex(System.Guid guid)
        {
            if (guid == System.Guid.Empty)
                return 0;
            
            if (!_guids.TryGetValue(guid, out uint index))
            {
                index = (uint) _rawStream.Length / 16 + 1;
                 AppendRawData(guid.ToByteArray());
                _guids.Add(guid, index);
            }

            return index;
        }

        /// <summary>
        /// Serializes the GUID stream buffer to a metadata stream.
        /// </summary>
        /// <returns>The metadata stream.</returns>
        public GuidStream CreateStream() => 
            new SerializedGuidStream(Name, _rawStream.ToArray());

        /// <inheritdoc />
        IMetadataStream IMetadataStreamBuffer.CreateStream() => CreateStream();
    }
}