using System;
using System.Collections.Generic;

namespace AsmResolver.DotNet.Extensions.Memory.Visitors
{
    internal abstract class VisitorBase : IVisitor
    {
        protected readonly Dictionary<FieldDefinition, uint> Offsets = new Dictionary<FieldDefinition, uint>();
        protected readonly TypeDefinition Parent;
        protected readonly bool Is32Bit;
        protected readonly uint Alignment;
        protected uint Size;
        
        protected VisitorBase(TypeDefinition parent, uint alignment, bool is32Bit)
        {
            Parent = parent;
            Alignment = alignment;
            Is32Bit = is32Bit;
        }

        public void VisitComplex(FieldNode node)
        {
            var resolved = node.Signature.Resolve();
            var visitor = node.IsExplicitLayout
                ? (VisitorBase) new ExplicitLayoutVisitor(resolved, Alignment, Is32Bit)
                : new SequentialLayoutVisitor(resolved, Alignment, Is32Bit);
            
            foreach (var child in node.Children)
                child.Accept(visitor);

            var layout = visitor.ConstructLayout(out _);
            CommenceInference(node.Field, layout.Size, layout.Offsets);
        }

        public void VisitPrimitive(FieldNode node)
        {
            CommenceInference(node.Field, node.Signature.SizeInBytes(Is32Bit));
        }

        protected abstract void CommenceInference(
            FieldDefinition field, uint inferredSize, IReadOnlyDictionary<FieldDefinition, uint> nested = null);

        internal TypeMemoryLayout ConstructLayout(out bool needsTypeAlignment)
        {
            // We first need to align the type it to its alignment
            // Even if the struct is seemingly empty, its size will be 1
            var inferredSize = Math.Max(1, Size); 
            
            // We try to get the explicit size, if there is none, we'll just use 0
            var explicitSize = Parent.ClassLayout?.ClassSize ?? 0;
            needsTypeAlignment = explicitSize < inferredSize;
            
            // If there is an explicitly set size for the struct, the "real" size is
            // the bigger one of the explicit size or the inferred size
            return new TypeMemoryLayout(Offsets, Math.Max(inferredSize, explicitSize));
        }
    }
}