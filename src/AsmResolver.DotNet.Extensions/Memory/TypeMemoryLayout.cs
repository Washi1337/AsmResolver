using System.Collections.Generic;

namespace AsmResolver.DotNet.Extensions.Memory
{
    /// <summary>
    /// Provides information about how a type is laid out in memory
    /// </summary>
    public sealed class TypeMemoryLayout
    {
        private readonly IReadOnlyDictionary<FieldDefinition, uint> _offsets;

        internal TypeMemoryLayout(IReadOnlyDictionary<FieldDefinition, uint> offsets, uint size)
        {
            _offsets = offsets;
            Size = size;
        }

        /// <summary>
        /// The size of the type in memory, in bytes
        /// </summary>
        public uint Size
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets the raw offset of the field in memory
        /// </summary>
        /// <param name="fieldDefinition">The field to get the offset of</param>
        /// <returns>The offset of the field, in bytes</returns>
        public uint GetFieldOffset(FieldDefinition fieldDefinition)
        {
            return _offsets[fieldDefinition];
        }
    }
}