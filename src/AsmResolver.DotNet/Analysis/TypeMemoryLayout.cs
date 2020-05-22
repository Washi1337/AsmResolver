using System.Collections.Generic;

namespace AsmResolver.DotNet.Analysis
{
    /// <summary>
    /// Provides information about the layout of a type
    /// </summary>
    public sealed class TypeMemoryLayout
    {
        private readonly IReadOnlyDictionary<FieldDefinition, int> _offsets;

        internal TypeMemoryLayout(IReadOnlyDictionary<FieldDefinition, int> offsets, int size)
        {
            _offsets = offsets;
            Size = size;
        }

        /// <summary>
        /// The size of a type in memory
        /// </summary>
        public int Size
        {
            get;
        }

        /// <summary>
        /// Gets the offset of a field in a type
        /// </summary>
        /// <param name="field">The field to get the offset of</param>
        /// <returns>The offset of the <paramref name="field"/></returns>
        public int GetFieldOffset(FieldDefinition field)
        {
            return _offsets[field];
        }
    }
}