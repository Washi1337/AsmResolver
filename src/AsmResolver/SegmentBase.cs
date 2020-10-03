namespace AsmResolver
{
    /// <summary>
    /// Provides a base for a segment in a file that can be relocated.
    /// </summary>
    public abstract class SegmentBase : ISegment
    {
        /// <inheritdoc />
        public ulong Offset
        {
            get;
            protected set;
        }

        /// <inheritdoc />
        public uint Rva
        {
            get;
            protected set;
        }

        /// <inheritdoc />
        public bool CanUpdateOffsets => true;

        /// <inheritdoc />
        public virtual void UpdateOffsets(ulong newOffset, uint newRva)
        {
            Offset = newOffset;
            Rva = newRva;
        }

        /// <inheritdoc />
        public abstract uint GetPhysicalSize();

        /// <inheritdoc />
        public uint GetVirtualSize() => GetPhysicalSize();

        /// <inheritdoc />
        public abstract void Write(IBinaryStreamWriter writer);
    }
}