using System;
using System.Collections.Generic;
using AsmResolver.IO;

namespace AsmResolver.PE.DotNet.Metadata
{
    /// <summary>
    /// Provides an implementation of a GUID stream that obtains GUIDs from a readable segment in a file.
    /// </summary>
    public class SerializedGuidStream : GuidStream
    {
        [ThreadStatic]
        private static byte[]? _readBuffer;

        private readonly BinaryStreamReader _reader;

        /// <summary>
        /// Creates a new GUID stream with the provided byte array as the raw contents of the stream.
        /// </summary>
        /// <param name="rawData">The raw contents of the stream.</param>
        public SerializedGuidStream(byte[] rawData)
            : this(DefaultName, new BinaryStreamReader(rawData))
        {
        }

        /// <summary>
        /// Creates a new GUID stream with the provided byte array as the raw contents of the stream.
        /// </summary>
        /// <param name="name">The name of the stream.</param>
        /// <param name="rawData">The raw contents of the stream.</param>
        public SerializedGuidStream(string name, byte[] rawData)
            : this(name, new BinaryStreamReader(rawData))
        {
        }

        /// <summary>
        /// Creates a new GUID stream with the provided file segment reader as the raw contents of the stream.
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
        public override void Write(BinaryStreamWriter writer) => _reader.Fork().WriteToOutput(writer);

        /// <inheritdoc />
        public override System.Guid GetGuidByIndex(uint index)
        {
            if (index == 0)
                return System.Guid.Empty;

            _readBuffer ??= new byte[GuidSize];

            uint offset = (index - 1) * GuidSize;
            if (offset < _reader.Length)
            {
                var guidReader = _reader.ForkRelative(offset);
                guidReader.ReadBytes(_readBuffer, 0, _readBuffer.Length);
                return new System.Guid(_readBuffer);
            }

            return System.Guid.Empty;
        }

        /// <inheritdoc />
        public override bool TryFindGuidIndex(System.Guid guid, out uint index)
        {
            _readBuffer ??= new byte[GuidSize];

            index = 1;
            var reader = _reader.Fork();
            while (reader.CanRead(GuidSize))
            {
                int count = reader.ReadBytes(_readBuffer, 0, _readBuffer.Length);
                if (count == GuidSize && new System.Guid(_readBuffer) == guid)
                    return true;

                index++;
            }

            index = 0;
            return false;
        }

        /// <inheritdoc />
        public override IEnumerable<Guid> EnumerateGuids()
        {
            int totalGuids = (int) (_reader.Length / GuidSize);
            for (int i = 0; i < totalGuids; i++)
                yield return GetGuidByIndex((uint) (i + 1));
        }
    }
}
