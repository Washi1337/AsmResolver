using System.Collections.Generic;

namespace AsmResolver.DotNet.Extensions.Memory
{
    /// <summary>
    /// Provides information about how a type is laid out in memory
    /// </summary>
    public sealed class TypeMemoryLayout
    {
        internal readonly IReadOnlyDictionary<FieldDefinition, uint> Offsets;

        internal TypeMemoryLayout(IReadOnlyDictionary<FieldDefinition, uint> offsets, uint size)
        {
            Offsets = offsets;
            Size = size;
        }

        /// <summary>
        /// The size of the type in memory, in bytes
        /// </summary>
        public uint Size
        {
            get;
        }

        /// <summary>
        /// Gets the raw offset of the field in memory
        /// </summary>
        /// <param name="fieldDefinition">The field to get the offset of</param>
        /// <returns>The offset of the field, in bytes</returns>
        public uint GetFieldOffset(FieldDefinition fieldDefinition)
        {
            return Offsets[fieldDefinition];
        }
    }
}