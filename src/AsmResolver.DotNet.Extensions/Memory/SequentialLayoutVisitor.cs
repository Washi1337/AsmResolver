using System;
using System.Collections.Generic;

namespace AsmResolver.DotNet.Extensions.Memory
{
    internal sealed class SequentialLayoutVisitor : VisitorBase
    {
        private readonly Dictionary<FieldDefinition, uint> _offsets = new Dictionary<FieldDefinition, uint>();
        private uint _size;

        internal SequentialLayoutVisitor(TypeDefinition parent, uint alignment, bool is32Bit)
            : base(parent, alignment, is32Bit) { }

        public override void VisitComplex(FieldNode node)
        {
            var layout = node.Signature.GetImpliedMemoryLayout(Is32Bit);
            var size = layout.Size;
            
            var alignment = Math.Min(Alignment, size);
            var aligned = size.Align(alignment);
            var padding = aligned - size;

            _size += padding;
            _offsets[node.Field] = _size;
            _size += size;
        }

        public override void VisitPrimitive(FieldNode node)
        {
            var size = node.Signature.SizeInBytes(Is32Bit);
            var alignment = Math.Min(Alignment, size);
            var aligned = size.Align(alignment);
            var padding = aligned - size;

            _size += padding;
            _offsets[node.Field] = _size;
            _size += size;
        }

        internal override TypeMemoryLayout ConstructLayout()
        {
            var realSize = _size.Align(Alignment);
            var explicitSize = Parent.ClassLayout?.ClassSize ?? 0;
            
            return new TypeMemoryLayout(_offsets, Math.Max(realSize, explicitSize));
        }
    }
}