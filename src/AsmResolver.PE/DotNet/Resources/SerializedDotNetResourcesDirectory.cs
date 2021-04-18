using System;
using AsmResolver.IO;

namespace AsmResolver.PE.DotNet.Resources
{
    /// <summary>
    /// Provides an implementation of a .NET resources directory that obtains blobs from a readable segment in a file.
    /// </summary>
    public class SerializedDotNetResourcesDirectory : DotNetResourcesDirectory
    {
        private readonly IReadableSegment _contents;

        /// <summary>
        /// Creates a new instance of the <see cref="SerializedDotNetResourcesDirectory"/> using an input stream as
        /// raw contents of the directory.
        /// </summary>
        /// <param name="reader">The input stream.</param>
        public SerializedDotNetResourcesDirectory(ref BinaryStreamReader reader)
        {
            if (!reader.IsValid)
                throw new ArgumentNullException(nameof(reader));
            _contents = DataSegment.FromReader(ref reader);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="SerializedDotNetResourcesDirectory"/> using a readable data segment
        /// as raw contents of the directory.
        /// </summary>
        /// <param name="contents">The input stream.</param>
        public SerializedDotNetResourcesDirectory(IReadableSegment contents)
        {
            _contents = contents ?? throw new ArgumentNullException(nameof(contents));
        }

        /// <inheritdoc />
        public override uint GetPhysicalSize() => _contents.GetPhysicalSize();

        /// <inheritdoc />
        public override void Write(IBinaryStreamWriter writer) => _contents.Write(writer);

        /// <inheritdoc />
        public override byte[] GetManifestResourceData(uint offset)
        {
            var reader = CreateManifestResourceReader(offset);
            if (!reader.IsValid)
                return null;

            var buffer = new byte[reader.Length];
            reader.ReadBytes(buffer, 0, buffer.Length);
            return buffer;
        }

        /// <inheritdoc />
        public override BinaryStreamReader CreateManifestResourceReader(uint offset)
        {
            if (offset >= _contents.GetPhysicalSize() - sizeof(uint))
                return default;

            var reader = _contents.CreateReader(_contents.Offset + offset);
            uint length = reader.ReadUInt32();
            return reader.ForkAbsolute(reader.Offset, length);
        }
    }
}
