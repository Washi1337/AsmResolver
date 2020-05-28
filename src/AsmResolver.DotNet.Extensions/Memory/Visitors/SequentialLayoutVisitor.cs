using System;
using System.Collections.Generic;

namespace AsmResolver.DotNet.Extensions.Memory.Visitors
{
    internal sealed class SequentialLayoutVisitor : VisitorBase
    {
        internal SequentialLayoutVisitor(TypeDefinition parent, uint alignment, bool is32Bit)
            : base(parent, alignment, is32Bit) { }

        protected override void CommenceInference(
            FieldDefinition field, uint inferredSize, IReadOnlyDictionary<FieldDefinition, uint> nested = null)
        {
            // https://docs.microsoft.com/en-us/dotnet/api/system.runtime.interopservices.structlayoutattribute.pack
            // Each field must align with fields of its own size (1, 2, 4, 8, etc., bytes)
            // or the alignment of the type, whichever is smaller.
            var alignment = Math.Min(Alignment, inferredSize);
            var aligned = inferredSize.Align(alignment);
            var padding = aligned - inferredSize;

            // First we add the padding so it aligns correctly
            Size += padding;
            
            // We can now set the offset, since it aligns on the boundary
            Offsets[field] = Size;
            
            // Carry over nested fields' offsets
            if (nested != null)
            {
                foreach (var pair in nested)
                {
                    Offsets[pair.Key] = Size + pair.Value;
                }
            }

            // Finally we add the actual inferred size
            Size += inferredSize;
        }
    }
}