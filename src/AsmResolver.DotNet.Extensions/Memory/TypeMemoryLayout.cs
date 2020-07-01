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
        
        
        public uint Size
        {
            get;
            internal set;
        }

        public IDictionary<FieldDefinition, FieldMemoryLayout> Fields
        {
            get;
        } = new Dictionary<FieldDefinition, FieldMemoryLayout>();
    }
}