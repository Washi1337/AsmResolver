using AsmResolver.Net.Emit;

namespace AsmResolver.Net.Signatures
{
    public abstract class BlobSignature
    {
        public abstract uint GetPhysicalLength(MetadataBuffer buffer);

        public abstract void Prepare(MetadataBuffer buffer);
        
        public abstract void Write(MetadataBuffer buffer, IBinaryStreamWriter writer);
    }
}
