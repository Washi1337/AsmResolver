using System;

namespace AsmResolver
{
    /// <summary>
    /// Represents a segment of code or data that might be resized at runtime.
    /// </summary>
    public class VirtualSegment : IReadableSegment
    {
        public VirtualSegment(ISegment physicalContents, uint virtualSize)
        {
            PhysicalContents = physicalContents ?? throw new ArgumentNullException(nameof(physicalContents));
            VirtualSize = virtualSize;
        }
        
        /// <summary>
        /// Gets or sets the physical contents of the segment as it is stored on the disk.
        /// </summary>
        public ISegment PhysicalContents
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
        public uint FileOffset => PhysicalContents.FileOffset;

        /// <inheritdoc />
        public uint Rva => PhysicalContents.Rva;

        /// <inheritdoc />
        public bool CanUpdateOffsets => PhysicalContents.CanUpdateOffsets;

        /// <summary>
        /// Gets a value indicating whether the physical contents of this segment is readable by a binary reader.
        /// </summary>
        public bool IsReadable => PhysicalContents is IReadableSegment;
        
        /// <inheritdoc />
        public void UpdateOffsets(uint newFileOffset, uint newRva) =>
            PhysicalContents.UpdateOffsets(newFileOffset, newRva);

        /// <inheritdoc />
        public uint GetPhysicalSize() => PhysicalContents.GetPhysicalSize();

        /// <inheritdoc />
        public uint GetVirtualSize() => VirtualSize;
        
        /// <inheritdoc />
        public IBinaryStreamReader CreateReader(uint fileOffset, uint size)
        {
            return PhysicalContents is IReadableSegment readableSegment
                ? readableSegment.CreateReader(fileOffset, size)
                : throw new InvalidOperationException("Physical contents of the virtual segment is not readable.");
        }

        /// <inheritdoc />
        public void Write(IBinaryStreamWriter writer) => PhysicalContents.Write(writer);
    }
}