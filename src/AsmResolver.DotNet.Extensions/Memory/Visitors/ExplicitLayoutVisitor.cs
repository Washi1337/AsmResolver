using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace AsmResolver.DotNet.Extensions.Memory.Visitors
{
    internal sealed class ExplicitLayoutVisitor : VisitorBase
    {
        private readonly Dictionary<FieldDefinition, uint> _offsets = new Dictionary<FieldDefinition, uint>();
        private uint _size;

        internal ExplicitLayoutVisitor(TypeDefinition parent, uint alignment, bool is32Bit)
            : base(parent, alignment, is32Bit) { }

        public override void VisitComplex(FieldNode node)
        {
            var layout = node.Signature.GetImpliedMemoryLayout(Is32Bit);
            CommenceInference(node.Field, layout.Size);
        }

        public override void VisitPrimitive(FieldNode node)
        {
            CommenceInference(node.Field, node.Signature.SizeInBytes(Is32Bit));
        }

        internal override TypeMemoryLayout ConstructLayout()
        {
            // We first need to align the type it to its alignment
            var inferredSize = _size.Align(Alignment); 
            
            // We try to get the explicit size, if there is none, we'll just use 0
            var explicitSize = Parent.ClassLayout?.ClassSize ?? 0; 
            
            // If there is an explicitly set size for the struct, the "real" size is
            // the bigger one of the explicit size or the inferred size
            return new TypeMemoryLayout(_offsets, Math.Max(inferredSize, explicitSize));
        }

        private void CommenceInference(FieldDefinition field, uint inferredSize)
        {
            Debug.Assert(field.FieldOffset != null, "A field in an explicit layout struct must have a FieldOffset");
            var offset = (uint) field.FieldOffset;

            // The offset is simply the explicit offset
            _offsets[field] = offset;
            
            // The size of an explicit struct is its largest field
            _size = Math.Max(_size, offset + inferredSize);
        }
    }
}