using AsmResolver.Net;
using AsmResolver.Net.Builder;

namespace AsmResolver
{
    public abstract class MethodBody : FileSegment
    {
        public abstract RvaDataSegment CreateDataSegment(MetadataBuffer buffer);
    }
}