using AsmResolver.Net.Builder;

namespace AsmResolver.Net.Signatures
{
    public abstract class BlobSignature
    {
        public abstract uint GetPhysicalLength();
        
        public abstract void Write(MetadataBuffer buffer, IBinaryStreamWriter writer);
    }
}
