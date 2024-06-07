using System;
using AsmResolver.IO;

namespace AsmResolver.PE.DotNet.Resources
{
    /// <summary>
    /// Provides an implementation of a .NET resources directory that obtains blobs from a readable segment in a file.
    /// </summary>
    public class SerializedDotNetResourcesDirectory : DotNetResourcesDirectory
    {
        private readonly BinaryStreamReader _reader;

        /// <summary>
        /// Creates a new instance of the <see cref="SerializedDotNetResourcesDirectory"/> using an input stream as
        /// raw contents of the directory.
        /// </summary>
        /// <param name="reader">The input stream.</param>
        public SerializedDotNetResourcesDirectory(in BinaryStreamReader reader)
        {
            if (!reader.IsValid)
                throw new ArgumentNullException(nameof(reader));
            _reader = reader;
            Offset = reader.Offset;
            Rva = reader.Rva;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="SerializedDotNetResourcesDirectory"/> using a readable data segment
        /// as raw contents of the directory.
        /// </summary>
        /// <param name="contents">The input stream.</param>
        public SerializedDotNetResourcesDirectory(IReadableSegment contents)
            : this(contents.CreateReader())
        {
        }

        /// <inheritdoc />
        public override uint GetPhysicalSize() => _reader.Length;

        /// <inheritdoc />
        public override void Write(BinaryStreamWriter writer) => _reader.Fork().WriteToOutput(writer);

        /// <inheritdoc />
        public override byte[]? GetManifestResourceData(uint offset)
        {
            if (!TryCreateManifestResourceReader(offset, out var reader))
                return null;

            byte[] buffer = new byte[reader.Length];
            reader.ReadBytes(buffer, 0, buffer.Length);
            return buffer;
        }

        /// <inheritdoc />
        public override bool TryCreateManifestResourceReader(uint offset, out BinaryStreamReader reader)
        {
            if (offset >= _reader.Length - sizeof(uint))
            {
                reader = default;
                return false;
            }

            reader = _reader.ForkRelative(offset);
            uint length = reader.ReadUInt32();

            if (!reader.CanRead(length))
            {
                reader = default;
                return false;
            }

            reader = reader.ForkAbsolute(reader.Offset, length);
            return true;
        }
    }
}
