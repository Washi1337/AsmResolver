using System;

namespace AsmResolver.PE.Relocations.Builder
{
    /// <summary>
    /// Represents one entry in a base relocation block, containing the offset within the page, as well as the type of relocation
    /// to apply after the PE image was loaded by the OS.
    /// </summary>
    public readonly struct RelocationEntry : IWritable
    {
        private readonly ushort _value;

        /// <summary>
        /// Creates a new relocation entry.
        /// </summary>
        /// <param name="value">The raw value of the entry.</param>
        public RelocationEntry(ushort value)
        {
            _value = value;
        }

        /// <summary>
        /// Creates a new relocation entry.
        /// </summary>
        /// <param name="type">The type of relocation to apply.</param>
        /// <param name="offset">The offset within the page to apply the relocation on.</param>
        /// <exception cref="ArgumentOutOfRangeException">Occurs when the offset does not indicate a valid offset within
        /// the page.</exception>
        public RelocationEntry(RelocationType type, int offset)
        {
            if (offset < 0)
                throw new ArgumentOutOfRangeException(nameof(offset), "Offset cannot be negative.");
            if (offset > 0xFFF)
                throw new ArgumentOutOfRangeException(nameof(offset), "Offset cannot be larger than 0xFFF.");
            
            _value = (ushort) (((byte) type << 12) | (offset & 0xFFF));
        }

        /// <summary>
        /// Gets the type of the relocation to apply.
        /// </summary>
        public RelocationType RelocationType => (RelocationType) (_value >> 12);

        /// <summary>
        /// Gets the offset (relative to the current relocation block) to the pointer to relocate.
        /// </summary>
        public int Offset => _value & 0xFFF;

        /// <inheritdoc />
        public uint GetPhysicalSize() => sizeof(ushort);

        /// <inheritdoc />
        public void Write(IBinaryStreamWriter writer) => writer.WriteUInt16(_value);
    }
}