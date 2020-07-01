using System.Collections.Generic;

namespace AsmResolver.DotNet.Memory
{
    /// <summary>
    /// Provides information about the memory layout of a type.
    /// </summary>
    public class TypeMemoryLayout
    {
        internal TypeMemoryLayout()
        {
        }
        
        /// <summary>
        /// Creates a new instance of the <see cref="TypeMemoryLayout"/> class.
        /// </summary>
        /// <param name="size">The size of the type.</param>
        public TypeMemoryLayout(uint size)
        {
            Size = size;
        }
        
        /// <summary>
        /// Gets the total number of bytes this structure requires.
        /// </summary>
        public uint Size
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets a dictionary of field definitions defined in the type for which the memory layout was determined.
        /// </summary>
        public IDictionary<FieldDefinition, FieldMemoryLayout> Fields
        {
            get;
        } = new Dictionary<FieldDefinition, FieldMemoryLayout>();
    }
}