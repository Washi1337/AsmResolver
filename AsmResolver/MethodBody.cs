using AsmResolver.Net;
using AsmResolver.Net.Emit;

namespace AsmResolver
{
    public abstract class MethodBody
    {
        public abstract uint GetCodeSize();
        
        public abstract FileSegment CreateRawMethodBody(MetadataBuffer buffer);
    }
}