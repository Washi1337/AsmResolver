using System;
using System.Diagnostics.CodeAnalysis;
using AsmResolver.IO;

namespace AsmResolver
{
    /// <summary>
    /// Represents a segment of code or data that might be resized at runtime.
    /// </summary>
    public class VirtualSegment : IReadableSegment
    {
        private uint _rva;

        /// <summary>
        /// Creates a new segment that is expanded at runtime to the provided virtual size.
        /// </summary>
        /// <param name="physicalContents">The physical contents of the segment.</param>
        /// <param name="virtualSize">The new size at runtime.</param>
        public VirtualSegment(ISegment? physicalContents, uint virtualSize)
        {
            PhysicalContents = physicalContents;
            VirtualSize = virtualSize;
        }

        /// <summary>
        /// Gets or sets the physical contents of the segment as it is stored on the disk.
        /// </summary>
        public ISegment? PhysicalContents
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the size of the segment after it is mapped into memory at runtime.
        /// </summary>
        public uint VirtualSize
        {
            get;
            set;
        }

        /// <inheritdoc />
        public ulong Offset => PhysicalContents?.Offset ?? 0;

        /// <inheritdoc />
        public uint Rva
        {
            get => _rva;
            set
            {
                _rva = value;
                PhysicalContents?.UpdateOffsets(Offset, value);
            }
        }

        /// <inheritdoc />
        public bool CanUpdateOffsets => PhysicalContents?.CanUpdateOffsets ?? false;

        /// <summary>
        /// Gets a value indicating whether the physical contents of this segment is readable by a binary reader.
        /// </summary>
        [MemberNotNullWhen(true, nameof(PhysicalContents))]
        public bool IsReadable => PhysicalContents is IReadableSegment;

        /// <inheritdoc />
        public void UpdateOffsets(ulong newOffset, uint newRva)
        {
            _rva = newRva;
            PhysicalContents?.UpdateOffsets(newOffset, newRva);
        }

        /// <inheritdoc />
        public uint GetPhysicalSize() => PhysicalContents?.GetPhysicalSize() ?? 0;

        /// <inheritdoc />
        public uint GetVirtualSize() => VirtualSize;

        /// <inheritdoc />
        public BinaryStreamReader CreateReader(ulong fileOffset, uint size)
        {
            return PhysicalContents is IReadableSegment readableSegment
                ? readableSegment.CreateReader(fileOffset, size)
                : throw new InvalidOperationException("Physical contents of the virtual segment is not readable.");
        }

        /// <inheritdoc />
        public void Write(IBinaryStreamWriter writer) => PhysicalContents?.Write(writer);
    }
}
