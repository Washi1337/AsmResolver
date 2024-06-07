using System;
using System.IO;
using AsmResolver.IO;

namespace AsmResolver
{
    /// <summary>
    /// Represents a segment containing zero bytes.
    /// </summary>
    public class ZeroesSegment : SegmentBase, IReadableSegment
    {
        private ZeroesDataSource? _dataSource;

        /// <summary>
        /// Creates a new zeroes-filled segment.
        /// </summary>
        /// <param name="size">The number of zero bytes.</param>
        public ZeroesSegment(uint size)
        {
            Size = size;
        }

        /// <summary>
        /// Gets the number of zero bytes that are stored in this segment.
        /// </summary>
        public uint Size
        {
            get;
        }

        /// <inheritdoc />
        public override uint GetPhysicalSize() => Size;

        /// <inheritdoc />
        public override void Write(BinaryStreamWriter writer) => writer.WriteZeroes((int) Size);

        /// <inheritdoc />
        public BinaryStreamReader CreateReader(ulong fileOffset, uint size)
        {
            if (fileOffset < Offset || fileOffset > Offset + Size)
                throw new ArgumentOutOfRangeException(nameof(fileOffset));
            if (fileOffset + size > Offset + Size)
                throw new EndOfStreamException();

            _dataSource ??= new ZeroesDataSource(Offset, Size);

            return new BinaryStreamReader(
                _dataSource,
                fileOffset,
                (uint) (fileOffset - Offset + Rva),
                size);
        }
    }
}
