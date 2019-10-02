using System;

namespace AsmResolver.PE.DotNet.Metadata
{
    /// <summary>
    /// Represents a metadata token, referencing a member using a table and a row index. 
    /// </summary>
    public readonly struct MetadataToken : IComparable<MetadataToken>
    {
        private readonly uint _value;

        /// <summary>
        /// Creates a new metadata token from a raw 32 bit integer.
        /// </summary>
        /// <param name="value">The raw metadata token.</param>
        public MetadataToken(uint value)
        {
            _value = value;
        }

        /// <summary>
        /// Creates a new metadata token from a table index and a row index.
        /// </summary>
        /// <param name="table">The table.</param>
        /// <param name="rid">The row index.</param>
        /// <exception cref="ArgumentOutOfRangeException">Occurs when <see cref="rid"/> is too large.</exception>
        public MetadataToken(MetadataTableIndex table, uint rid)
            : this((uint) (((byte)table << 24) | rid & 0x00FFFFFF))
        {
            if (rid > 0x00FFFFFF)
                throw new ArgumentOutOfRangeException(nameof(rid));
        }

        /// <summary>
        /// Gets the table that the metadata token references.
        /// </summary>
        public MetadataTableIndex Table => (MetadataTableIndex) (_value >> 24);

        /// <summary>
        /// Gets the row index within the table specified by <see cref="Table"/> that the metadata token references.
        /// </summary>
        public uint Rid => _value & 0x00FFFFFF;

        /// <summary>
        /// Converts the metadata token to an unsigned 32 bit integer.
        /// </summary>
        /// <returns>The raw metadata token.</returns>
        public uint ToUInt32() => _value;

        /// <summary>
        /// Converts the metadata token to a signed 32 bit integer.
        /// </summary>
        /// <returns>The raw metadata token.</returns>
        public int ToInt32() => unchecked((int) _value);

        /// <inheritdoc />
        public override string ToString()
        {
            return _value.ToString("X8");
        }

        /// <summary>
        /// Determines whether the metadata token refers to the same member as another metadata token.
        /// </summary>
        /// <param name="other">The other metadata token.</param>
        /// <returns><c>true</c> if the token refers to the same member, <c>false</c> otherwise.</returns>
        public bool Equals(MetadataToken other)
        {
            return _value == other._value;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is MetadataToken other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return (int) _value;
        }

        /// <inheritdoc />
        public int CompareTo(MetadataToken other)
        {
            return _value.CompareTo(other._value);
        }
        
    }
}