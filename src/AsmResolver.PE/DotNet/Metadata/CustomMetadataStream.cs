using System;

namespace AsmResolver.PE.DotNet.Metadata
{
    /// <summary>
    /// Represents a metadata stream with contents in a custom data format.
    /// </summary>
    public class CustomMetadataStream : IMetadataStream
    {
        /// <summary>
        /// Creates a new custom metadata stream.
        /// </summary>
        /// <param name="name">The name of the stream.</param>
        /// <param name="data">The raw contents of the stream.</param>
        public CustomMetadataStream(string name, byte[] data)
            : this(name, new DataSegment(data))
        {
        }

        /// <summary>
        /// Creates a new custom metadata stream.
        /// </summary>
        /// <param name="name">The name of the stream.</param>
        /// <param name="contents">The raw contents of the stream.</param>
        public CustomMetadataStream(string name, ISegment contents)
        {
            Name = name;
            Contents = contents;
        }

        /// <inheritdoc />
        public uint FileOffset => Contents.FileOffset;

        /// <inheritdoc />
        public uint Rva => Contents.Rva;

        /// <inheritdoc />
        public bool CanUpdateOffsets => true;

        /// <inheritdoc />
        public string Name
        {
            get;
            set;
        }

        /// <inheritdoc />
        public bool CanRead => Contents is IReadableSegment;

        /// <summary>
        /// Gets or sets the raw contents of the stream.
        /// </summary>
        public ISegment Contents
        {
            get;
            set;
        }

        /// <inheritdoc />
        public IBinaryStreamReader CreateReader()
        {
            if (!CanRead)
                throw new InvalidOperationException("Contents of the metadata stream is not readable.");
            return ((IReadableSegment) Contents).CreateReader();
        }

        /// <inheritdoc />
        public void UpdateOffsets(uint newFileOffset, uint newRva) => Contents.UpdateOffsets(newFileOffset, newRva);

        /// <inheritdoc />
        public uint GetPhysicalSize() => Contents.GetPhysicalSize();

        /// <inheritdoc />
        public uint GetVirtualSize() => GetPhysicalSize();

        /// <inheritdoc />
        public void Write(IBinaryStreamWriter writer) => Contents.Write(writer);
    }
}