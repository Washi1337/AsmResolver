using AsmResolver.Net.Emit;
using AsmResolver.Net.Cts;
using AsmResolver.Net.Metadata;

namespace AsmResolver.Net.Signatures
{
    public class FunctionPointerTypeSignature : TypeSignature
    {
        public static FunctionPointerTypeSignature FromReader(MetadataImage image, IBinaryStreamReader reader)
        {
            return FromReader(image, reader, new RecursionProtection());
        }
        
        public static FunctionPointerTypeSignature FromReader(
            MetadataImage image, 
            IBinaryStreamReader reader, 
            RecursionProtection protection)
        {
            return new FunctionPointerTypeSignature(MethodSignature.FromReader(image, reader, false, protection));
        }

        public FunctionPointerTypeSignature(MethodSignature signature)
        {
            Signature = signature;
        }

        public override ElementType ElementType => ElementType.FnPtr;

        public MethodSignature Signature
        {
            get;
            set;
        }

        public override string Name => "*";

        public override string Namespace => string.Empty;

        public override string FullName => Signature.ToString();

        public override IResolutionScope ResolutionScope => null;

        public override uint GetPhysicalLength(MetadataBuffer buffer)
        {
            return sizeof(byte)
                   + Signature.GetPhysicalLength(buffer)
                   + base.GetPhysicalLength(buffer);
        }

        public override void Prepare(MetadataBuffer buffer)
        {
            Signature.Prepare(buffer);
        }

        public override void Write(MetadataBuffer buffer, IBinaryStreamWriter writer)
        {
            writer.WriteByte((byte) ElementType.FnPtr);
            Signature.Write(buffer, writer);

            base.Write(buffer, writer);
        }
    }
}
