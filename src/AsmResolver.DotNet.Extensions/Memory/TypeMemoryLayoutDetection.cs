using AsmResolver.DotNet.Signatures.Types;

namespace AsmResolver.DotNet.Memory
{
    public static class TypeMemoryLayoutDetection
    {
        public static TypeMemoryLayout GetImpliedMemoryLayout(this TypeSignature type, bool is32Bit)
        {
            var layoutDetector = new TypeMemoryLayoutDetector(is32Bit);
            return type.AcceptVisitor(layoutDetector);
        }
        
        public static TypeMemoryLayout GetImpliedMemoryLayout(this TypeDefinition type, bool is32Bit)
        {
            var layoutDetector = new TypeMemoryLayoutDetector(is32Bit);
            return layoutDetector.VisitTypeDefinition(type);
        }
    }
}