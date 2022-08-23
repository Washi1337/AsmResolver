namespace AsmResolver
{
    /// <summary>
    /// Provides parameters for relocating a segment to a new offset-rva pair.
    /// </summary>
    public struct RelocationParameters
    {
        /// <summary>
        /// Creates new relocation parameters.
        /// </summary>
        /// <param name="offset">The new offset of the segment.</param>
        /// <param name="rva">The new virtual address of the segment, relative to the image base.</param>
        public RelocationParameters(ulong offset, uint rva)
            : this(0, offset, rva, true)
        {
        }

        /// <summary>
        /// Creates new relocation parameters.
        /// </summary>
        /// <param name="imageBase">The base address of the image the segment is located in.</param>
        /// <param name="offset">The new offset of the segment.</param>
        /// <param name="rva">The new virtual address of the segment, relative to the image base.</param>
        /// <param name="is32Bit"><c>true</c> if the image is targeting 32-bit images, <c>false</c> for 64-bit images.</param>
        public RelocationParameters(ulong imageBase, ulong offset, uint rva, bool is32Bit)
        {
            ImageBase = imageBase;
            Offset = offset;
            Rva = rva;
            Is32Bit = is32Bit;
        }

        /// <summary>
        /// Gets the image base that is assumed when relocating the segment.
        /// </summary>
        public ulong ImageBase
        {
            get;
        }

        /// <summary>
        /// Gets the new physical offset of the segment.
        /// </summary>
        public ulong Offset
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the new virtual address of the segment, relative to the image base.
        /// </summary>
        public uint Rva
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a value indicating whether the image is targeting 32-bit machines.
        /// </summary>
        public bool Is32Bit
        {
            get;
        }

        /// <summary>
        /// Gets a value indicating whether the image is targeting 64-bit machines.
        /// </summary>
        public bool Is64Bit => !Is32Bit;

        /// <summary>
        /// Copies the current relocation parameters, and assigns a new offset and relative virtual address.
        /// </summary>
        /// <param name="offset">The new offset.</param>
        /// <param name="rva">The new relative virtual address.</param>
        public RelocationParameters WithOffsetRva(ulong offset, uint rva)
        {
            return new RelocationParameters(ImageBase, offset, rva, Is32Bit);
        }

        /// <summary>
        /// Aligns the current offset and virtual address to the nearest multiple of the provided alignment.
        /// </summary>
        /// <param name="alignment">The alignment.</param>
        public void Align(uint alignment)
        {
            Offset = Offset.Align(alignment);
            Rva = Rva.Align(alignment);
        }

        /// <summary>
        /// Advances the current offset and virtual address by the provided byte count.
        /// </summary>
        /// <param name="count">The number of bytes to advance with.</param>
        /// <returns>The new relocation parameters.</returns>
        public readonly RelocationParameters WithAdvance(uint count)
        {
            return new RelocationParameters(ImageBase, Offset + count, Rva + count, Is32Bit);
        }

        /// <summary>
        /// Advances the current offset and virtual address by the provided byte count.
        /// </summary>
        /// <param name="count">The number of bytes to advance with.</param>
        /// <returns>The new relocation parameters.</returns>
        public void Advance(uint count)
        {
            Offset += count;
            Rva += count;
        }

        /// <summary>
        /// Advances the current offset and virtual address by the provided byte count.
        /// </summary>
        /// <param name="physicalCount">The number of bytes to advance the physical offset with.</param>
        /// <param name="virtualCount">The number of bytes to advance the virtual address with.</param>
        public void Advance(uint physicalCount, uint virtualCount)
        {
            Offset += physicalCount;
            Rva += virtualCount;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{nameof(ImageBase)}: {ImageBase:X8}, {nameof(Offset)}: {Offset:X8}, {nameof(Rva)}: {Rva:X8}";
        }
    }
}
