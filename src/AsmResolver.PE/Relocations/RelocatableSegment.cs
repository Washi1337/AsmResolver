using System.Collections.Generic;

namespace AsmResolver.PE.Relocations
{
    /// <summary>
    /// Pairs a segment with relocation information.
    /// </summary>
    public readonly struct RelocatableSegment
    {
        /// <summary>
        /// Creates a new pairing between a segment and relocation information.
        /// </summary>
        /// <param name="segment">The segment.</param>
        /// <param name="relocations">The relocation information.</param>
        public RelocatableSegment(ISegment segment, IList<BaseRelocation> relocations)
        {
            Segment = segment;
            Relocations = relocations;
        }

        /// <summary>
        /// Gets the segment that is relocatable.
        /// </summary>
        public ISegment Segment
        {
            get;
        }

        /// <summary>
        /// Gets the relocation information required to relocate the segment.
        /// </summary>
        public IList<BaseRelocation> Relocations
        {
            get;
        }
    }
}
