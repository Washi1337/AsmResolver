using AsmResolver.Net;
using AsmResolver.Net.Emit;

namespace AsmResolver
{
    public abstract class MethodBody : FileSegment
    {
        public abstract RvaDataSegment CreateDataSegment(MetadataBuffer buffer);
    }
}