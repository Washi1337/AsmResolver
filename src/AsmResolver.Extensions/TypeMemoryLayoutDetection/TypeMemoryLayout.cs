using System.Collections.Generic;
using AsmResolver.DotNet;

namespace AsmResolver.Extensions.TypeMemoryLayoutDetection
{
    public sealed class TypeMemoryLayout
    {
        private readonly IReadOnlyDictionary<FieldDefinition, int> _offsets;

        internal TypeMemoryLayout(IReadOnlyDictionary<FieldDefinition, int> offsets, int size)
        {
            _offsets = offsets;
            Size = size;
        }

        public int Size
        {
            get;
        }

        public int GetFieldOffset(FieldDefinition fieldDefinition)
        {
            return _offsets[fieldDefinition];
        }
    }
}