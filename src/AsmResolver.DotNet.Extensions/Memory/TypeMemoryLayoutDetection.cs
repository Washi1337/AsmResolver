using AsmResolver.DotNet.Signatures.Types;

namespace AsmResolver.DotNet.Memory
{
    public static class TypeMemoryLayoutDetection
    {
        public static TypeMemoryLayout GetImpliedMemoryLayout(this TypeSignature type, bool is32Bit)
        {
            var alignment = TypeAlignmentDetector.GetTypeAlignment(type, is32Bit);
            var layoutDetector = new TypeMemoryLayoutDetector(is32Bit, alignment);
            return type.AcceptVisitor(layoutDetector);
        }
        
        public static TypeMemoryLayout GetImpliedMemoryLayout(this TypeDefinition type, bool is32Bit)
        {
            var alignment = TypeAlignmentDetector.GetTypeAlignment(type, is32Bit);
            var layoutDetector = new TypeMemoryLayoutDetector(is32Bit, alignment);
            return layoutDetector.VisitTypeDefinition(type);
        }
    }
}