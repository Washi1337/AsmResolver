using System.Collections.Generic;

namespace AsmResolver.PE.Relocations
{
    public readonly struct RelocatableSegment
    {
        public RelocatableSegment(ISegment segment, IReadOnlyList<BaseRelocation> relocations)
        {
            Segment = segment;
            Relocations = relocations;
        }

        public ISegment Segment
        {
            get;
        }

        public IReadOnlyList<BaseRelocation> Relocations
        {
            get;
        }
    }
}
