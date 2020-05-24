using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures.Types;

namespace AsmResolver.Extensions.TypeMemoryLayoutDetection
{
    public static class TypeMemoryLayoutDetector
    {
        public static TypeMemoryLayout GetImpliedMemoryLayout(this TypeDefinition typeDefinition, bool is32Bit)
        {
            return GetImpliedMemoryLayout(typeDefinition.ToTypeSignature(), is32Bit);
        }

        public static TypeMemoryLayout GetImpliedMemoryLayout(this TypeSpecification typeSpecification, bool is32Bit)
        {
            return GetImpliedMemoryLayout(typeSpecification.Signature, is32Bit);
        }

        public static TypeMemoryLayout GetImpliedMemoryLayout(this TypeSignature typeSignature, bool is32Bit)
        {
            return new TypeMemoryLayout(null, 0);
        }
    }
}