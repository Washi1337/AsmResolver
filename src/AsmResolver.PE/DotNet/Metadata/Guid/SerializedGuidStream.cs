using System;
using AsmResolver.IO;

namespace AsmResolver.PE.DotNet.Metadata.Guid
{
    /// <summary>
    /// Provides an implementation of a GUID stream that obtains GUIDs from a readable segment in a file.
    /// </summary>
    public class SerializedGuidStream : GuidStream
    {
        private readonly BinaryStreamReader _reader;

        /// <summary>
        /// Creates a new GUID stream based on a byte array.
        /// </summary>
        /// <param name="name">The name of the stream.</param>
        /// <param name="rawData">The raw contents of the stream.</param>
        public SerializedGuidStream(string name, byte[] rawData)
            : this(name, ByteArrayDataSource.CreateReader(rawData))
        {
        }

        /// <summary>
        /// Creates a new GUID stream based on a segment in a file.
        /// </summary>
        /// <param name="name">The name of the stream.</param>
        /// <param name="reader">The raw contents of the stream.</param>
        public SerializedGuidStream(string name, in BinaryStreamReader reader)
            : base(name)
        {
            _reader = reader;
            Offset = reader.Offset;
            Rva = reader.Rva;
        }

        /// <inheritdoc />
        public override bool CanRead => true;

        /// <inheritdoc />
        public override BinaryStreamReader CreateReader() => _reader.Fork();

        /// <inheritdoc />
        public override uint GetPhysicalSize() => _reader.Length;

        /// <inheritdoc />
        public override void Write(IBinaryStreamWriter writer) => _reader.Fork().WriteToOutput(writer);

        /// <inheritdoc />
        public override System.Guid GetGuidByIndex(uint index)
        {
            if (index == 0)
                return System.Guid.Empty;

            uint offset = (index - 1) * GuidSize;
            if (offset < _reader.Length)
            {
                var guidReader = _reader.ForkRelative(offset);
                byte[] data = new byte[16];
                guidReader.ReadBytes(data, 0, data.Length);
                return new System.Guid(data);
            }

            return System.Guid.Empty;
        }

    }
}
