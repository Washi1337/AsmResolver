using System;

namespace AsmResolver.PE.Relocations
{
    /// <summary>
    /// Represents a single base relocation that is applied after the operating system has loaded the PE image into
    /// memory.
    /// </summary>
    public readonly struct BaseRelocation
    {
        /// <summary>
        /// Creates a new base relocation.
        /// </summary>
        /// <param name="type">The type of base relocation to apply.</param>
        /// <param name="location">The location within the executable to apply the base relocation.</param>
        public BaseRelocation(RelocationType type, ISegmentReference location)
        {
            Type = type;
            Location = location ?? throw new ArgumentNullException(nameof(location));
        }
        
        /// <summary>
        /// Gets the type of relocation to apply.
        /// </summary>
        public RelocationType Type
        {
            get;
        }
        
        /// <summary>
        /// Gets the location within the executable to apply the relocation to.
        /// </summary>
        public ISegmentReference Location
        {
            get;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Location.Rva:X8} ({Type})";
        }

        /// <summary>
        /// Determines whether two base relocations are considered equal.
        /// </summary>
        /// <param name="other">The other base relocation.</param>
        /// <returns><c>true</c> if the base relocations are equal, <c>false</c> otherwise.</returns>
        /// <remarks>
        /// This method only considers the virtual address (RVA) of the target location, and not the entire
        /// <see cref="ISegmentReference"/> member.
        /// </remarks>
        public bool Equals(BaseRelocation other)
        {
            return Type == other.Type && Location.Rva == other.Location.Rva;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is BaseRelocation other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                return ((int) Type * 397) ^ (int) Location.Rva;
            }
        }
        
    }
}