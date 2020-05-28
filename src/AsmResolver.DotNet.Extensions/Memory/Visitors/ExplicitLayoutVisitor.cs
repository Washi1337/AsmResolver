using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace AsmResolver.DotNet.Extensions.Memory.Visitors
{
    internal sealed class ExplicitLayoutVisitor : VisitorBase
    {
        internal ExplicitLayoutVisitor(TypeDefinition parent, uint alignment, bool is32Bit)
            : base(parent, alignment, is32Bit) { }


        protected override void CommenceInference(
            FieldDefinition field, uint inferredSize, IReadOnlyDictionary<FieldDefinition, uint> nested = null)
        {
            Debug.Assert(field.FieldOffset != null, "A field in an explicit layout struct must have a FieldOffset");
            var offset = (uint) field.FieldOffset;

            // The offset is simply the explicit offset
            Offsets[field] = offset;
            
            // Carry over nested fields' offsets
            if (nested != null)
            {
                foreach (var pair in nested)
                {
                    Offsets[pair.Key] = Size + pair.Value;
                }
            }
            
            // The size of an explicit struct is its largest field
            Size = Math.Max(Size, offset + inferredSize);
        }
    }
}