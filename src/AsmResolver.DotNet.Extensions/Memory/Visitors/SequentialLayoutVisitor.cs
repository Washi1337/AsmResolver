﻿using System;
using System.Collections.Generic;

namespace AsmResolver.DotNet.Extensions.Memory.Visitors
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
            CommenceInference(node.Field, layout.Size, layout.Offsets);
        }

        public override void VisitPrimitive(FieldNode node)
        {
            CommenceInference(node.Field, node.Signature.SizeInBytes(Is32Bit));
        }

        internal override TypeMemoryLayout ConstructLayout()
        {
            // We first need to align the type it to its alignment
            // Even if the struct is seemingly empty, its size will be 1
            var inferredSize = Math.Max(1, _size.Align(Alignment)); 
            
            // We try to get the explicit size, if there is none, we'll just use 0
            var explicitSize = Parent.ClassLayout?.ClassSize ?? 0; 
            
            // If there is an explicitly set size for the struct, the "real" size is
            // the bigger one of the explicit size or the inferred size
            return new TypeMemoryLayout(_offsets, Math.Max(inferredSize, explicitSize));
        }

        private void CommenceInference(
            FieldDefinition field, uint inferredSize, IReadOnlyDictionary<FieldDefinition, uint> nested = null)
        {
            // https://docs.microsoft.com/en-us/dotnet/api/system.runtime.interopservices.structlayoutattribute.pack
            // Each field must align with fields of its own size (1, 2, 4, 8, etc., bytes)
            // or the alignment of the type, whichever is smaller.
            var alignment = Math.Min(Alignment, inferredSize);
            var aligned = inferredSize.Align(alignment);
            var padding = aligned - inferredSize;

            // First we add the padding so it aligns correctly
            _size += padding;
            
            // We can now set the offset, since it aligns on the boundary
            _offsets[field] = _size;
            
            // Carry over nested fields' offsets
            if (nested != null)
            {
                foreach (var pair in nested)
                {
                    _offsets[pair.Key] = _size + pair.Value;
                }
            }

            // Finally we add the actual inferred size
            _size += inferredSize;
        }
    }
}