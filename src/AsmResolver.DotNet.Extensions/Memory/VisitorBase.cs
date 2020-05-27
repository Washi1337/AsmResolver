using System.Collections.Generic;
using AsmResolver.DotNet.Signatures.Types;

namespace AsmResolver.DotNet.Extensions.Memory
{
    internal abstract class VisitorBase : IVisitor
    {
        protected readonly TypeDefinition Parent;
        protected readonly bool Is32Bit;
        protected readonly uint Alignment;
        
        protected VisitorBase(TypeDefinition parent, uint alignment, bool is32Bit)
        {
            Parent = parent;
            Alignment = alignment;
            Is32Bit = is32Bit;
        }

        public virtual void VisitComplex(FieldNode node)
        {
            foreach (var child in node.Children)
                child.Accept(this);
        }

        public virtual void VisitPrimitive(FieldNode node) { }

        internal abstract TypeMemoryLayout ConstructLayout();

        protected void EnsureCycle(HashSet<TypeSignature> visited, TypeSignature current)
        {
            if (!visited.Add(current))
                throw new TypeMemoryLayoutDetectionException("Cyclic dependency in graph");
        }
    }
}