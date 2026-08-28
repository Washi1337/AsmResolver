using System;
using AsmResolver.IO;

namespace AsmResolver.PE.Debug
{
    public class PortablePdbDataSegment : SegmentBase, IDebugDataSegment
    {
        public const uint MAGIC = 0x4244504d;

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

        public override uint GetPhysicalSize() => sizeof(uint) + sizeof(int) + CompressedContents.GetPhysicalSize();

        public override void UpdateOffsets(in RelocationParameters parameters) => CompressedContents.UpdateOffsets(parameters.WithAdvance(sizeof(uint) + sizeof(int)));

        /// <inheritdoc />
        public override void Write(BinaryStreamWriter writer)
        {
            writer.WriteUInt32(MAGIC);
            writer.WriteInt32(UncompressedSize);
            CompressedContents.Write(writer);
        }

        public static PortablePdbDataSegment FromReader(PEReaderContext context, ref BinaryStreamReader reader)
        {
            if (reader.ReadUInt32() != MAGIC)
            {
                throw new BadImageFormatException("Embedded PDB section without correct magic");
            }
            return new PortablePdbDataSegment(reader.ReadInt32(), DataSegment.FromReader(ref reader));
        }
    }
}
