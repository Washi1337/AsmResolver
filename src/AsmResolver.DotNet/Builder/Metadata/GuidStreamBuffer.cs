using System;
using System.Collections.Generic;
using System.IO;
using AsmResolver.IO;
using AsmResolver.PE.DotNet.Metadata;

namespace AsmResolver.DotNet.Builder.Metadata
{
    /// <summary>
    /// Provides a mutable buffer for building up a GUID stream in a .NET portable executable.
    /// </summary>
    public class GuidStreamBuffer : IMetadataStreamBuffer
    {
        private readonly MemoryStream _rawStream = new();
        private readonly BinaryStreamWriter _writer;
        private readonly Dictionary<System.Guid, uint> _guids = new();

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

        /// <inheritdoc />
        public bool IsEmpty => _rawStream.Length <= 0;

        /// <summary>
        /// Imports the contents of a GUID stream and indexes all present GUIDs.
        /// </summary>
        /// <param name="stream">The stream to import.</param>
        public void ImportStream(GuidStream stream)
        {
            uint index = 1;
            while (index < stream.GetPhysicalSize() / GuidStream.GuidSize + 1)
            {
                var guid = stream.GetGuidByIndex(index);
                uint newIndex = AppendGuid(guid);
                _guids[guid] = newIndex;

                index++;
            }
        }

        private uint AppendRawData(byte[] data)
        {
            uint offset = (uint) _rawStream.Length;
            _writer.WriteBytes(data, 0, data.Length);
            return offset;
        }

        private uint AppendGuid(System.Guid guid)
        {
            uint index = (uint) _rawStream.Length / GuidStream.GuidSize + 1;
            AppendRawData(guid.ToByteArray());
            return index;
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
                index = AppendGuid(guid);
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
