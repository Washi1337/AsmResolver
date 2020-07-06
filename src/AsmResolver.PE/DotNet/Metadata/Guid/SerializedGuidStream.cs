using System;

namespace AsmResolver.PE.DotNet.Metadata.Guid
{
    /// <summary>
    /// Provides an implementation of a GUID stream that obtains GUIDs from a readable segment in a file.  
    /// </summary>
    public class SerializedGuidStream : GuidStream
    {
        private readonly IReadableSegment _contents;

        /// <summary>
        /// Creates a new GUID stream based on a byte array.
        /// </summary>
        /// <param name="name">The name of the stream.</param>
        /// <param name="rawData">The raw contents of the stream.</param>
        public SerializedGuidStream(string name, byte[] rawData)
            : this(name, new DataSegment(rawData))
        {
        }

        /// <summary>
        /// Creates a new GUID stream based on a segment in a file.
        /// </summary>
        /// <param name="name">The name of the stream.</param>
        /// <param name="contents">The raw contents of the stream.</param>
        public SerializedGuidStream(string name, IReadableSegment contents)
            : base(name)
        {
            _contents = contents ?? throw new ArgumentNullException(nameof(contents));
        }

        /// <inheritdoc />
        public override bool CanRead => true;

        /// <inheritdoc />
        public override IBinaryStreamReader CreateReader() => _contents.CreateReader();

        /// <inheritdoc />
        public override uint GetPhysicalSize() => _contents.GetPhysicalSize();

        /// <inheritdoc />
        public override void Write(IBinaryStreamWriter writer) => _contents.Write(writer);

        /// <inheritdoc />
        public override System.Guid GetGuidByIndex(uint index)
        {
            if (index == 0)
                return System.Guid.Empty;

            uint offset = (index - 1) * GuidSize;
            if (offset < _contents.GetPhysicalSize())
            {
                var guidReader = _contents.CreateReader((uint) (_contents.FileOffset + offset));
                var data = new byte[16];
                guidReader.ReadBytes(data, 0, data.Length);
                return new System.Guid(data);
            }

            return System.Guid.Empty;
        }
        
    }
}