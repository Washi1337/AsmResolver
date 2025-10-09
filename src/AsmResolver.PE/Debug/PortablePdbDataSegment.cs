using System;
using AsmResolver.IO;

namespace AsmResolver.PE.Debug
{
    public class PortablePdbDataSegment : IDebugDataSegment, IReadableSegment
    {
        /// <summary>
        /// Creates a new instance of the <see cref="CustomDebugDataSegment"/> class.
        /// </summary>
        /// <param name="type">The format of the data.</param>
        /// <param name="contents">The contents of the code.</param>
        public PortablePdbDataSegment(int uncompressedSize, IReadableSegment contents)
        {
            UncompressedSize = uncompressedSize;
            CompressedContents = contents ?? throw new ArgumentNullException(nameof(contents));
        }

        /// <inheritdoc />
        public DebugDataType Type => DebugDataType.EmbeddedPortablePdb;

        public int UncompressedSize { get; set; }

        /// <summary>
        /// Gets or sets the raw compressed data of the segment.
        /// </summary>
        public IReadableSegment CompressedContents
        {
            get;
            set;
        }

        /// <inheritdoc />
        public ulong Offset => CompressedContents.Offset;

        /// <inheritdoc />
        public uint Rva => CompressedContents.Rva;

        /// <inheritdoc />
        public bool CanUpdateOffsets => CompressedContents?.CanUpdateOffsets ?? false;

        /// <inheritdoc />
        public void UpdateOffsets(in RelocationParameters parameters) => CompressedContents.UpdateOffsets(parameters);

        /// <inheritdoc />
        public BinaryStreamReader CreateReader(ulong fileOffset, uint size) => CompressedContents.CreateReader(fileOffset, size);

        /// <inheritdoc />
        public uint GetPhysicalSize() => CompressedContents.GetPhysicalSize();

        /// <inheritdoc />
        public uint GetVirtualSize() => CompressedContents.GetPhysicalSize();

        /// <inheritdoc />
        public void Write(BinaryStreamWriter writer)
        {
            writer.WriteUInt32(0x4244504d);
            writer.WriteInt32(UncompressedSize);
            CompressedContents.Write(writer);
        }

        public static PortablePdbDataSegment FromReader(PEReaderContext context, ref BinaryStreamReader reader)
        {
            if (reader.ReadUInt32() != 0x4244504d)
            {
                throw new BadImageFormatException("Embedded PDB section without correct magic");
            }
            return new PortablePdbDataSegment(reader.ReadInt32(), DataSegment.FromReader(ref reader));
        }
    }
}
