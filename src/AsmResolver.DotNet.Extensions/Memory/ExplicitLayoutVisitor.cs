using System;
using System.Collections.Generic;

namespace AsmResolver.DotNet.Extensions.Memory
{
    internal sealed class ExplicitLayoutVisitor : VisitorBase
    {
        private readonly Dictionary<FieldDefinition, uint> _offsets = new Dictionary<FieldDefinition, uint>();
        private uint _max;

        internal ExplicitLayoutVisitor(TypeDefinition parent, uint alignment, bool is32Bit)
            : base(parent, alignment, is32Bit) { }

        public override void VisitComplex(FieldNode node)
        {
            var layout = node.Signature.GetImpliedMemoryLayout(Is32Bit);
            var offset = node.ExplicitOffset ?? 0;
            _offsets[node.Field] = offset;
            _max = Math.Max(_max, offset + layout.Size);
        }

        public override void VisitPrimitive(FieldNode node)
        {
            var explicitOffset = node.ExplicitOffset ?? 0;

            _offsets[node.Field] = explicitOffset;
            _max = Math.Max(_max, explicitOffset + node.Signature.SizeInBytes(Is32Bit));
        }

        internal override TypeMemoryLayout ConstructLayout()
        {
            var realSize = _max.Align(Alignment);
            var explicitSize = Parent.ClassLayout?.ClassSize ?? 0;
            
            return new TypeMemoryLayout(_offsets, Math.Max(realSize, explicitSize));
        }
    }
}