using System;
using System.IO;
using AsmResolver.IO;

namespace AsmResolver
{
    /// <summary>
    /// Represents a segment that originates from a slice of a <see cref="IDataSource"/>.
    /// </summary>
    public class DataSourceSegment : SegmentBase, IReadableSegment
    {
        private readonly IDataSource _dataSource;
        private readonly ulong _originalOffset;
        private readonly uint _originalSize;

        private DisplacedDataSource? _displacedDataSource;

        /// <inheritdoc />
        public DataSourceSegment(IDataSource dataSource, ulong offset, uint rva, uint size)
        {
            _dataSource = dataSource;
            Offset = _originalOffset = offset;
            Rva = rva;
            _originalSize = size;
        }

        /// <inheritdoc />
        public override void UpdateOffsets(in RelocationParameters parameters)
        {
            base.UpdateOffsets(parameters);

            long displacement = (long) parameters.Offset - (long) _originalOffset;
            _displacedDataSource = displacement != 0
                ? new DisplacedDataSource(_dataSource, displacement)
                : null;
        }

        /// <inheritdoc />
        public override uint GetPhysicalSize() => _originalSize;

        /// <inheritdoc />
        public override void Write(BinaryStreamWriter writer) =>
            CreateReader(Offset, _originalSize).WriteToOutput(writer);

        /// <inheritdoc />
        public BinaryStreamReader CreateReader(ulong fileOffset, uint size)
        {
            if (fileOffset < Offset || fileOffset > Offset + _dataSource.Length)
                throw new ArgumentOutOfRangeException(nameof(fileOffset));
            if (fileOffset + size > Offset + _dataSource.Length)
                throw new EndOfStreamException();

            return new BinaryStreamReader(
                _displacedDataSource ?? _dataSource,
                fileOffset,
                (uint) (fileOffset - Offset + Rva),
                size);
        }
    }
}
