using AsmResolver.Net.Emit;

namespace AsmResolver.Net.Signatures
{
    public abstract class BlobSignature
    {
        public abstract uint GetPhysicalLength(MetadataBuffer buffer);

        public abstract void Prepare(MetadataBuffer buffer);
        
        public abstract void Write(MetadataBuffer buffer, IBinaryStreamWriter writer);
    }

    public abstract class ExtendableBlobSignature : BlobSignature
    {
        public byte[] ExtraData
        {
            get;
            set;
        }
        
        public override uint GetPhysicalLength(MetadataBuffer buffer)
        {
            return (uint) (ExtraData != null ? ExtraData.Length : 0);
        }

        public override void Write(MetadataBuffer buffer, IBinaryStreamWriter writer)
        {
            if (ExtraData != null)
                writer.WriteBytes(ExtraData);
        }
    }
}
