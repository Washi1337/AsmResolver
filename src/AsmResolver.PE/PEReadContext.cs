using System;

namespace AsmResolver.PE
{
    public class PEReadContext
    {
        public PEReadContext(ISegmentReferenceResolver referenceResolver)
            : this(referenceResolver, new PEReadParameters())
        {
        }

        public PEReadContext(ISegmentReferenceResolver referenceResolver, PEReadParameters parameters)
        {
            ReferenceResolver = referenceResolver;
            Parameters = parameters;
        }
        
        public ISegmentReferenceResolver ReferenceResolver
        {
            get;
        }

        public PEReadParameters Parameters
        {
            get;
        }
    }
}