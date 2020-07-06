using System;

namespace AsmResolver.PE.DotNet.Metadata
{
    /// <summary>
    /// Represents a raw metadata stream header, defining the offset, size and name of a metadata stream.
    /// </summary>
    public readonly struct MetadataStreamHeader
    {
        /// <summary>
        /// Reads a single metadata stream header from an input stream.
        /// </summary>
        /// <param name="reader">The input stream.</param>
        /// <returns>The header.</returns>
        public static MetadataStreamHeader FromReader(IBinaryStreamReader reader)
        {
            uint offset = reader.ReadUInt32();
            uint size = reader.ReadUInt32();
            string name = reader.ReadAsciiString();
            reader.Align(4);
            return new MetadataStreamHeader(offset, size, name);
        }
        
        /// <summary>
        /// Creates a new metadata stream header.
        /// </summary>
        /// <param name="offset">The offset to the contents of the stream.</param>
        /// <param name="size">The size in bytes of the contents.</param>
        /// <param name="name">The name of the stream.</param>
        /// <exception cref="ArgumentOutOfRangeException">Occurs when the name is too long.</exception>
        public MetadataStreamHeader(uint offset, uint size, string name)
        {
            Offset = offset;
            Size = size;
            Name = name ?? throw new ArgumentNullException(nameof(name));
            
            if (name.Length > 32)
                throw new ArgumentOutOfRangeException(nameof(name), "Name can be no longer than 32 bytes.");
        }
        
        /// <summary>
        /// Gets the offset (relative to the start of the metadata directory) referencing the beginning of the contents
        /// of the stream.
        /// </summary>
        public uint Offset
        {
            get;
        }

        /// <summary>
        /// Gets the number of bytes the stream contains.
        /// </summary>
        public uint Size
        {
            get;
        }

        /// <summary>
        /// Gets the name of the stream.
        /// </summary>
        public string Name
        {
            get;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{nameof(Offset)}: {Offset:X8}, {nameof(Size)}: {Size:X8}, {nameof(Name)}: {Name}";
        }
    }
}